using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Entities;
using PosnaiSQLauncher.Helpers;
using PosnaiSQLauncher.Services;

namespace PosnaiSQLauncher
{
    public partial class EditOptionView : UserControl
    {
        private MainWindow _parent;
        private readonly AppDbContext _context;
        
        private Option _currentOption;
        private string _selectedImagePath;
        private string _selectedLocation;
        private int _currentTimeLimit = 300;
        private bool _isAutoLoading = false;

        public EditOptionView(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            _context = new AppDbContext();

            this.Loaded += EditOptionView_Loaded;
        }

        private void ShowLoading(string message)
        {
            LoadingOverlayContainer.Children.Clear();
            LoadingOverlayContainer.Children.Add(new LoadingOverlay(message));
            LoadingOverlayContainer.Visibility = Visibility.Visible;
        }

        private void HideLoading()
        {
            LoadingOverlayContainer.Visibility = Visibility.Collapsed;
        }

        private async void EditOptionView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                ShowLoading("Загрузка данных...");

                var options = await _context.Options
                    .Include(o => o.IdQueryNavigation)
                    .ToListAsync();

                VariantComboBox.Items.Clear();
                foreach (var opt in options)
                {
                    string queryName = opt.IdQueryNavigation?.Name ?? $"Без запроса (ID: {opt.IdOption})";
                    VariantComboBox.Items.Add(new ComboBoxItem 
                    { 
                        Content = $"Вариант №{opt.IdOption} — {queryName}", 
                        Tag = opt.IdOption 
                    });
                }

                var databases = await _context.Databases.OrderBy(d => d.Name).ToListAsync();
                DatabaseComboBox.Items.Clear();
                foreach (var db in databases)
                {
                    DatabaseComboBox.Items.Add(new ComboBoxItem { Content = db.Name, Tag = db.IdDatabase });
                }

                var queries = await _context.Queries.OrderBy(q => q.Name).ToListAsync();
                QueryComboBox.Items.Clear();
                QueryComboBox.Items.Add(new ComboBoxItem { Content = "Новый запрос (пустой)", Tag = 0 });
                foreach (var q in queries)
                {
                    QueryComboBox.Items.Add(new ComboBoxItem { Content = q.Name, Tag = q.IdQuery });
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка инициализации данных: {ex.Message}");
            }
            finally
            {
                HideLoading();
            }
        }

        private void VariantComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = VariantComboBox.SelectedItem != null;
            OpenVariantButton.IsEnabled = hasSelection;
            DeleteVariantButton.IsEnabled = hasSelection;
        }

private async void OpenVariant_Click(object sender, RoutedEventArgs e)
{
    if (VariantComboBox.SelectedItem is not ComboBoxItem selectedItem || selectedItem.Tag is not int optionId)
        return;

    OpenVariantButton.IsEnabled = false;
    DeleteVariantButton.IsEnabled = false;

    try
    {
        ShowLoading("Открытие варианта...");

        _currentOption = await _context.Options
            .Include(o => o.IdQueryNavigation)
            .ThenInclude(q => q.IdDatabaseNavigation)
            .FirstOrDefaultAsync(o => o.IdOption == optionId);

        if (_currentOption == null)
        {
            MessageBoxHelper.ShowError("Выбранный вариант не найден в БД.");
            await LoadInitialDataAsync();
            return;
        }

        _isAutoLoading = true;

        _currentTimeLimit = _currentOption.TimeLimit;
        UpdateTimeDisplay();

        if (_currentOption.IdLocation == 1) SelectLocationCard("desert");
        else if (_currentOption.IdLocation == 2) SelectLocationCard("forest");

        if (_currentOption.IdQueryNavigation != null)
        {
            SelectComboBoxItemByTag(QueryComboBox, _currentOption.IdQuery);
            QueryNameTextBox.Text = _currentOption.IdQueryNavigation.Name;
            QueryConditionTextBox.Text = _currentOption.IdQueryNavigation.Condition;
            QuerySqlTextBox.Text = _currentOption.IdQueryNavigation.QueryString;

            if (_currentOption.IdQueryNavigation.IdDatabaseNavigation != null)
            {
                var db = _currentOption.IdQueryNavigation.IdDatabaseNavigation;
                SelectComboBoxItemByTag(DatabaseComboBox, db.IdDatabase);
                DatabaseNameTextBox.Text = db.Name;

                if (!string.IsNullOrEmpty(db.SchemaImage))
                {
                    ImagePreview.Source = ImageService.Base64ToImage(db.SchemaImage);
                    ImagePlaceholder.Visibility = Visibility.Collapsed;
                }
            }
        }

        VariantOverlay.Visibility = Visibility.Collapsed;
        UpdateVariantTitle(optionId);
    }
    catch (Exception ex)
    {
        MessageBoxHelper.ShowError($"Ошибка при открытии варианта: {ex.Message}");
    }
    finally
    {
        _isAutoLoading = false;
        HideLoading(); 
        
        bool hasSelection = VariantComboBox.SelectedItem != null;
        OpenVariantButton.IsEnabled = hasSelection;
        DeleteVariantButton.IsEnabled = hasSelection;
    }
}        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                int textLength = textBox.Text.Length;
                if (textLength < 100) textBox.FontSize = 25;
                else if (textLength < 300) textBox.FontSize = 20;
                else if (textLength < 600) textBox.FontSize = 15;
                else textBox.FontSize = 12;
            }
        }
        
        public void UpdateVariantTitle(int variantNumber)
        {
            HeaderTitleTextBlock.Visibility = Visibility.Visible;
            HeaderTitleTextBlock.Text = $"Редактирование варианта № {variantNumber}";
        }

        private async void DeleteVariant_Click(object sender, RoutedEventArgs e)
        {
            if (VariantComboBox.SelectedItem is not ComboBoxItem selectedItem || selectedItem.Tag is not int optionId)
                return;

            var confirm = MessageBoxHelper.ShowQuestion("Вы уверены, что хотите удалить этот игровой вариант?", "Удаление варианта");
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var option = await _context.Options.FindAsync(optionId);
                if (option != null)
                {
                    _context.Options.Remove(option);
                    await _context.SaveChangesAsync();
                }

                MessageBoxHelper.ShowSuccess("Вариант успешно удален.");
                await LoadInitialDataAsync();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка при удалении: {ex.Message}");
            }
        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasDb = DatabaseComboBox.SelectedItem != null;
            DeleteDatabaseButton.IsEnabled = hasDb;
    
            if (DatabaseComboBox.SelectedItem is ComboBoxItem item && item.Tag is int dbId)
            {
                var db = _context.Databases.Local.FirstOrDefault(d => d.IdDatabase == dbId) 
                         ?? _context.Databases.FirstOrDefault(d => d.IdDatabase == dbId);
        
                if (db != null)
                {
                    DatabaseNameTextBox.Text = db.Name;

                    if (!_isAutoLoading)
                    {
                        QueryComboBox.SelectedItem = null;
                        QueryNameTextBox.Text = "";
                        QueryConditionTextBox.Text = "";
                        QuerySqlTextBox.Text = "";
                    }

                    if (!string.IsNullOrEmpty(db.SchemaImage))
                    {
                        ImagePreview.Source = ImageService.Base64ToImage(db.SchemaImage);
                        ImagePlaceholder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        ImagePreview.Source = null;
                        ImagePlaceholder.Visibility = Visibility.Visible;
                    }
                }
            }
        }
        private async void DeleteDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (DatabaseComboBox.SelectedItem is not ComboBoxItem item || item.Tag is not int dbId) return;

            try
            {
                var queries = await _context.Queries.Where(q => q.IdDatabase == dbId).ToListAsync();
                var queryIds = queries.Select(q => q.IdQuery).ToList();
                int optionsCount = await _context.Options.CountAsync(o => o.IdQuery != null && queryIds.Contains(o.IdQuery.Value));

                string msg = $"Удаление БД повлечет за собой полное удаление:\n" +
                             $"• Всех связанных запросов ({queries.Count} шт.)\n" +
                             $"• Всех связанных вариантов задач ({optionsCount} шт.)\n\n" +
                             $"Вы действительно хотите полностью удалить эту структуру?";

                if (MessageBoxHelper.ShowQuestion(msg, "Удаление") != MessageBoxResult.Yes) return;

                var relatedOptions = await _context.Options.Where(o => o.IdQuery != null && queryIds.Contains(o.IdQuery.Value)).ToListAsync();
                if (relatedOptions.Any()) _context.Options.RemoveRange(relatedOptions);
                if (queries.Any()) _context.Queries.RemoveRange(queries);

                var db = await _context.Databases.FindAsync(dbId);
                if (db != null) _context.Databases.Remove(db);

                await _context.SaveChangesAsync();
                
                MessageBoxHelper.ShowSuccess("База данных и вся её структура успешно стерты.");
                _parent.ShowMainMenu();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка полного удаления: {ex.Message}");
            }
        }

        private void AddDatabase_Click(object sender, RoutedEventArgs e)
        {
            DatabaseComboBox.SelectedItem = null;
            DatabaseNameTextBox.Text = "Новая БД";
            ImagePreview.Source = null;
            ImagePlaceholder.Visibility = Visibility.Visible;
            _selectedImagePath = null;
            DatabaseNameTextBox.Focus();
        }

        private void QueryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QueryComboBox.SelectedItem is not ComboBoxItem item) return;
            
            int queryId = (int)item.Tag;
            DeleteQueryButton.IsEnabled = queryId > 0;

            if (queryId > 0)
            {
                var query = _context.Queries.FirstOrDefault(q => q.IdQuery == queryId);
                if (query != null)
                {
                    QueryNameTextBox.Text = query.Name;
                    QueryConditionTextBox.Text = query.Condition;
                    QuerySqlTextBox.Text = query.QueryString;
                }
            }
            else
            {
                QueryNameTextBox.Clear();
                QueryConditionTextBox.Clear();
                QuerySqlTextBox.Clear();
            }
        }

        private async void DeleteQuery_Click(object sender, RoutedEventArgs e)
        {
            if (QueryComboBox.SelectedItem is not ComboBoxItem item || item.Tag is not int queryId) return;

            try
            {
                int optionsCount = await _context.Options.CountAsync(o => o.IdQuery == queryId);

                string msg = $"Удаление запроса полностью уничтожит все привязанные к нему игровые варианты ({optionsCount} шт.). Продолжить?";
                if (MessageBoxHelper.ShowQuestion(msg, "Полное удаление запроса") != MessageBoxResult.Yes) return;

                var relatedOptions = await _context.Options.Where(o => o.IdQuery == queryId).ToListAsync();
                if (relatedOptions.Any()) _context.Options.RemoveRange(relatedOptions);

                var query = await _context.Queries.FindAsync(queryId);
                if (query != null) _context.Queries.Remove(query);

                await _context.SaveChangesAsync();
                
                MessageBoxHelper.ShowSuccess("Запрос удален.");
                _parent.ShowMainMenu();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка удаления запроса: {ex.Message}");
            }
        }

        private void AddQuery_Click(object sender, RoutedEventArgs e)
        {
            SelectComboBoxItemByTag(QueryComboBox, 0);
            QueryNameTextBox.Text = "Новый запрос";
            QueryConditionTextBox.Clear();
            QuerySqlTextBox.Clear();
            QueryNameTextBox.Focus();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DatabaseNameTextBox.Text) || string.IsNullOrWhiteSpace(QueryNameTextBox.Text))
            {
                MessageBoxHelper.ShowWarning("Поля имен базы данных и запроса обязательны для заполнения.");
                return;
            }

            try
            {
                Database dbEntity;
                if (DatabaseComboBox.SelectedItem is ComboBoxItem dbItem && dbItem.Tag is int dbId && dbId > 0)
                {
                    dbEntity = await _context.Databases.FindAsync(dbId);
                    if (dbEntity != null)
                    {
                        dbEntity.Name = DatabaseNameTextBox.Text.Trim();
                        if (!string.IsNullOrEmpty(_selectedImagePath))
                            dbEntity.SchemaImage = ImageService.ImageToBase64(_selectedImagePath);
                    }
                }
                else
                {
                    dbEntity = new Database
                    {
                        Name = DatabaseNameTextBox.Text.Trim(),
                        SchemaImage = !string.IsNullOrEmpty(_selectedImagePath) ? ImageService.ImageToBase64(_selectedImagePath) : null
                    };
                    _context.Databases.Add(dbEntity);
                    await _context.SaveChangesAsync(); 
                }

                Query queryEntity;
                if (QueryComboBox.SelectedItem is ComboBoxItem qItem && qItem.Tag is int qId && qId > 0)
                {
                    queryEntity = await _context.Queries.FindAsync(qId);
                    if (queryEntity != null)
                    {
                        queryEntity.Name = QueryNameTextBox.Text.Trim();
                        queryEntity.Condition = QueryConditionTextBox.Text.Trim();
                        queryEntity.QueryString = QuerySqlTextBox.Text.Trim();
                        queryEntity.IdDatabase = dbEntity.IdDatabase;
                    }
                }
                else
                {
                    queryEntity = new Query
                    {
                        Name = QueryNameTextBox.Text.Trim(),
                        Condition = QueryConditionTextBox.Text.Trim(),
                        QueryString = QuerySqlTextBox.Text.Trim(),
                        IdDatabase = dbEntity.IdDatabase
                    };
                    _context.Queries.Add(queryEntity);
                    await _context.SaveChangesAsync();
                }

                if (_currentOption != null)
                {
                    _currentOption.IdQuery = queryEntity.IdQuery;
                    _currentOption.TimeLimit = _currentTimeLimit;
                    _currentOption.IdLocation = (_selectedLocation == "forest") ? 2 : 1;

                    _context.Options.Update(_currentOption);
                    await _context.SaveChangesAsync();
                }

                MessageBoxHelper.ShowSuccess("Конфигурация варианта успешно синхронизирована!");
                _parent.ShowMainMenu();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp" };
            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;
                ImagePreview.Source = new BitmapImage(new Uri(_selectedImagePath));
                ImagePlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void CardDesert_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) => SelectLocationCard("desert");
        private void CardForest_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) => SelectLocationCard("forest");

        private void SelectLocationCard(string location)
        {
            _selectedLocation = location;
            var activeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
            var inactiveColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));

            CardDesert.BorderBrush = (location == "desert") ? activeColor : inactiveColor;
            CardForest.BorderBrush = (location == "forest") ? activeColor : inactiveColor;
        }

        private void TimeMinus30_Click(object sender, RoutedEventArgs e) { _currentTimeLimit = Math.Max(10, _currentTimeLimit - 30); UpdateTimeDisplay(); }
        private void TimeMinus5_Click(object sender, RoutedEventArgs e) { _currentTimeLimit = Math.Max(5, _currentTimeLimit - 5); UpdateTimeDisplay(); }
        private void TimePlus5_Click(object sender, RoutedEventArgs e) { _currentTimeLimit += 5; UpdateTimeDisplay(); }
        private void TimePlus30_Click(object sender, RoutedEventArgs e) { _currentTimeLimit += 30; UpdateTimeDisplay(); }

        private void UpdateTimeDisplay()
        {
            MinDisplay.Text = (_currentTimeLimit / 60).ToString("D2");
            SecDisplay.Text = (_currentTimeLimit % 60).ToString("D2");
        }

        private void Back_Click(object sender, RoutedEventArgs e) => _parent.ShowMainMenu();

        private void SelectComboBoxItemByTag(ComboBox comboBox, int? tagValue)
        {
            if (tagValue == null) return;
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Tag is int val && val == tagValue.Value)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }
    }
}