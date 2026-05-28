using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Microsoft.EntityFrameworkCore;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Entities;
using PosnaiSQLauncher.Services;
using PosnaiSQLauncher.Helpers; 

namespace PosnaiSQLauncher
{
    public partial class EditOptionView : UserControl
    {
        private MainWindow _parent;
        private AppDbContext _context;
        private DatabaseService _databaseService;
        private QueryService _queryService;
        private OptionService _optionService;
        private LocationService _locationService;

        private Option _currentOption;
        private int _timeLimitSeconds = 300; 
        private int? _selectedLocationId = 1; 
        private string _currentImagePath = null; 
        
        private bool _isPopulating = false; 

        public EditOptionView(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;

            _context = new AppDbContext();
            _databaseService = new DatabaseService(_context);
            _queryService = new QueryService(_context);
            _optionService = new OptionService(_context);
            _locationService = new LocationService(_context);

            this.Loaded += EditOptionView_Loaded;
        }

        private async void EditOptionView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadOptionsListAsync();
            VariantOverlay.Visibility = Visibility.Visible;
        }

        private async Task LoadOptionsListAsync()
        {
            try
            {
                var options = await _context.Options
                    .Include(o => o.IdQueryNavigation)
                    .ThenInclude(q => q.IdDatabaseNavigation)
                    .ToListAsync();

                VariantComboBox.Items.Clear();

                foreach (var option in options)
                {
                    var queryName = option.IdQueryNavigation?.Name ?? "Неизвестный запрос";
                    var dbName = option.IdQueryNavigation?.IdDatabaseNavigation?.Name ?? "Неизвестная БД";
                    
                    VariantComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = $"Вариант {option.IdOption}: {queryName} ({dbName})",
                        Tag = option 
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка при загрузке вариантов: {ex.Message}", "Ошибка");
            }
        }
        
        private void AddDatabase_Click(object sender, RoutedEventArgs e)
        {
            _isPopulating = true;
    
            DatabaseComboBox.SelectedItem = null;
            DatabaseNameTextBox.Text = string.Empty;
            ImagePreview.Source = null;
            ImagePlaceholder.Visibility = Visibility.Visible;
            _currentImagePath = null;
            DeleteDatabaseButton.IsEnabled = false; 

            _isPopulating = false;

            ClearQueryFields();
        }

        private void AddQuery_Click(object sender, RoutedEventArgs e)
        {
            ClearQueryFields();
        }

        private void ClearQueryFields()
        {
            _isPopulating = true;
    
            QueryComboBox.SelectedItem = null;
            QueryNameTextBox.Text = string.Empty;
            QueryConditionTextBox.Text = string.Empty;
            QuerySqlTextBox.Text = string.Empty;
            DeleteQueryButton.IsEnabled = false;

            _isPopulating = false;
        }

        private void VariantComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isSelected = VariantComboBox.SelectedItem != null;
            OpenVariantButton.IsEnabled = isSelected;
            DeleteVariantButton.IsEnabled = isSelected;
        }

        private async void OpenVariant_Click(object sender, RoutedEventArgs e)
        {
            if (VariantComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is Option option)
            {
                try
                {
                    _currentOption = await _optionService.GetByIdAsync(option.IdOption);
                    
                    await PopulateMainFormAsync();

                    VariantOverlay.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBoxHelper.ShowError($"Ошибка открытия: {ex.Message}", "Ошибка");
                }
            }
        }

        private async void DeleteVariant_Click(object sender, RoutedEventArgs e)
        {
            if (VariantComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is Option option)
            {
                var result = MessageBoxHelper.ShowQuestion("Вы уверены, что хотите удалить этот вариант?", "Подтверждение");
                if (result == MessageBoxResult.Yes)
                {
                    await _optionService.DeleteAsync(option.IdOption);
                    await LoadOptionsListAsync(); 
                }
            }
        }

        private async Task PopulateMainFormAsync()
        {
            if (_currentOption == null) return;

            _isPopulating = true; 
            
            try
            {
                var query = _currentOption.IdQueryNavigation;
                var database = query?.IdDatabaseNavigation;

                await LoadDatabasesAsync();
                await LoadQueriesAsync(database?.IdDatabase ?? 0);

                if (database != null)
                {
                    SelectInComboBox(DatabaseComboBox, database.IdDatabase);
                    DatabaseNameTextBox.Text = database.Name;
                    
                    if (!string.IsNullOrEmpty(database.SchemaImage))
                    {
                        ImagePreview.Source = ImageService.Base64ToImage(database.SchemaImage);
                        ImagePlaceholder.Visibility = Visibility.Collapsed;
                    }
                    DeleteDatabaseButton.IsEnabled = true;
                }

                if (query != null)
                {
                    SelectInComboBox(QueryComboBox, query.IdQuery);
                    QueryNameTextBox.Text = query.Name;
                    QueryConditionTextBox.Text = query.Condition;
                    QuerySqlTextBox.Text = query.QueryString;
                    DeleteQueryButton.IsEnabled = true;
                }

                _selectedLocationId = _currentOption.IdLocation;
                UpdateLocationVisuals();

                _timeLimitSeconds = _currentOption.TimeLimit;
                UpdateTimerDisplay();
            }
            finally
            {
                _isPopulating = false; 
            }
        }

        private void SelectInComboBox(ComboBox comboBox, int id)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Tag is int itemId && itemId == id)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private async Task LoadDatabasesAsync()
        {
            DatabaseComboBox.SelectionChanged -= DatabaseComboBox_SelectionChanged; 
            DatabaseComboBox.Items.Clear();

            var databases = await _databaseService.GetAllAsync();
            foreach (var db in databases)
            {
                DatabaseComboBox.Items.Add(new ComboBoxItem { Content = db.Name, Tag = db.IdDatabase });
            }
            DatabaseComboBox.SelectionChanged += DatabaseComboBox_SelectionChanged;
        }

        private async Task LoadQueriesAsync(int dbId)
        {
            QueryComboBox.SelectionChanged -= QueryComboBox_SelectionChanged;
            QueryComboBox.Items.Clear();

            var queries = await _queryService.GetByDatabaseIdAsync(dbId);
            foreach (var q in queries)
            {
                QueryComboBox.Items.Add(new ComboBoxItem { Content = q.Name, Tag = q.IdQuery });
            }
            QueryComboBox.SelectionChanged += QueryComboBox_SelectionChanged;
        }

        private async void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isPopulating) return; 

            if (DatabaseComboBox.SelectedItem is ComboBoxItem item && item.Tag is int dbId)
            {
                var db = await _databaseService.GetByIdAsync(dbId);
                DatabaseNameTextBox.Text = db.Name;
                
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

                _currentImagePath = null; 
                await LoadQueriesAsync(dbId);
            }
        }

        private async void QueryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isPopulating) return; 

            if (QueryComboBox.SelectedItem is ComboBoxItem item && item.Tag is int queryId)
            {
                var query = await _queryService.GetByIdAsync(queryId);
                QueryNameTextBox.Text = query.Name;
                QueryConditionTextBox.Text = query.Condition;
                QuerySqlTextBox.Text = query.QueryString;
            }
        }

private async void DeleteDatabase_Click(object sender, RoutedEventArgs e)
{
    if (DatabaseComboBox.SelectedItem is ComboBoxItem item && item.Tag is int dbId)
    {
        var result = MessageBoxHelper.ShowQuestion(
            "Удаление БД приведет к безвозвратному удалению ВСЕХ её запросов и ВСЕХ связанных игровых вариантов, включая ТЕКУЩИЙ. Вы уверены?", 
            "Полное удаление");
            
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var queries = await _context.Queries.Where(q => q.IdDatabase == dbId).ToListAsync();
                var queryIds = queries.Select(q => q.IdQuery).ToList();

                var relatedOptions = await _context.Options
                    .Where(o => o.IdQuery != null && queryIds.Contains(o.IdQuery.Value))
                    .ToListAsync();

                if (relatedOptions.Any())
                {
                    _context.Options.RemoveRange(relatedOptions);
                }

                if (queries.Any())
                {
                    _context.Queries.RemoveRange(queries);
                }

                var db = await _context.Databases.FindAsync(dbId);
                if (db != null)
                {
                    _context.Databases.Remove(db);
                }

                await _context.SaveChangesAsync();
                
                MessageBoxHelper.ShowSuccess("База данных, её запросы и связанные варианты успешно удалены!", "Успех");
                
                _parent.ShowMainMenu();
            }
            catch (Exception ex) 
            { 
                MessageBoxHelper.ShowError($"Ошибка каскадного удаления БД: {ex.Message}", "Ошибка"); 
            }
        }
    }
}
        private async void DeleteQuery_Click(object sender, RoutedEventArgs e)
        {
            if (QueryComboBox.SelectedItem is ComboBoxItem item && item.Tag is int queryId)
            {
                var result = MessageBoxHelper.ShowQuestion(
                    "Удаление запроса приведет к безвозвратному удалению ВСЕХ связанных с ним игровых вариантов, включая ТЕКУЩИЙ. Вы уверены?", 
                    "Полное удаление");
            
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var relatedOptions = await _context.Options.Where(o => o.IdQuery == queryId).ToListAsync();
                        if (relatedOptions.Any())
                        {
                            _context.Options.RemoveRange(relatedOptions);
                        }

                        var query = await _context.Queries.FindAsync(queryId);
                        if (query != null)
                        {
                            _context.Queries.Remove(query);
                        }

                        await _context.SaveChangesAsync();
                
                        MessageBoxHelper.ShowSuccess("Запрос и все связанные варианты успешно удалены!", "Успех");
                
                        _parent.ShowMainMenu();
                    }
                    catch (Exception ex) 
                    { 
                        MessageBoxHelper.ShowError($"Ошибка каскадного удаления запроса: {ex.Message}", "Ошибка"); 
                    }
                }
            }
        }
        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Выберите схему базы данных"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _currentImagePath = openFileDialog.FileName;
                
                var uri = new Uri(_currentImagePath);
                ImagePreview.Source = new System.Windows.Media.Imaging.BitmapImage(uri);
                ImagePlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void CardDesert_Click(object sender, MouseButtonEventArgs e)
        {
            _selectedLocationId = 1; 
            UpdateLocationVisuals();
        }

        private void CardForest_Click(object sender, MouseButtonEventArgs e)
        {
            _selectedLocationId = 2; 
            UpdateLocationVisuals();
        }

        private void UpdateLocationVisuals()
        {
            CardDesert.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
            CardForest.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));

            if (_selectedLocationId == 1)
                CardDesert.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100")); 
            else if (_selectedLocationId == 2)
                CardForest.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")); 
        }

        private void TimeMinus30_Click(object sender, RoutedEventArgs e) { ChangeTime(-30); }
        private void TimeMinus5_Click(object sender, RoutedEventArgs e) { ChangeTime(-5); }
        private void TimePlus5_Click(object sender, RoutedEventArgs e) { ChangeTime(5); }
        private void TimePlus30_Click(object sender, RoutedEventArgs e) { ChangeTime(30); }

        private void ChangeTime(int seconds)
        {
            _timeLimitSeconds += seconds;
            if (_timeLimitSeconds < 0) _timeLimitSeconds = 0;
            if (_timeLimitSeconds > 3599) _timeLimitSeconds = 3599; 
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            int minutes = _timeLimitSeconds / 60;
            int seconds = _timeLimitSeconds % 60;

            MinDisplay.Text = minutes.ToString("D2");
            SecDisplay.Text = seconds.ToString("D2");
        }

private async void Save_Click(object sender, RoutedEventArgs e)
{
    if (_currentOption == null) return;

    try
    {
        int finalDbId = 0;

        if (DatabaseComboBox.SelectedItem is ComboBoxItem dbItem && dbItem.Tag is int dbId)
        {
            await _databaseService.UpdateAsync(dbId, DatabaseNameTextBox.Text, _currentImagePath);
            finalDbId = dbId;
        }
        else if (!string.IsNullOrWhiteSpace(DatabaseNameTextBox.Text))
        {
            var newDb = await _databaseService.CreateAsync(DatabaseNameTextBox.Text, _currentImagePath);
            finalDbId = newDb.IdDatabase;
        }
        else
        {
            MessageBoxHelper.ShowWarning("Имя базы данных не может быть пустым!", "Предупреждение");
            return;
        }

        int finalQueryId = 0;
        if (QueryComboBox.SelectedItem is ComboBoxItem qItem && qItem.Tag is int queryId)
        {
            await _queryService.UpdateAsync(queryId, QueryNameTextBox.Text, QueryConditionTextBox.Text, QuerySqlTextBox.Text);
            finalQueryId = queryId;
        }
        else if (!string.IsNullOrWhiteSpace(QueryNameTextBox.Text))
        {
            var newQuery = await _queryService.CreateAsync(finalDbId, QueryNameTextBox.Text, QueryConditionTextBox.Text, QuerySqlTextBox.Text);
            finalQueryId = newQuery.IdQuery;
        }
        else
        {
            MessageBoxHelper.ShowWarning("Имя запроса не может быть пустым!", "Предупреждение");
            return;
        }


        _currentOption.IdLocation = _selectedLocationId;
        _currentOption.TimeLimit = _timeLimitSeconds;
        _currentOption.IdQuery = finalQueryId; 

        _context.Options.Update(_currentOption);
        await _context.SaveChangesAsync();

        MessageBoxHelper.ShowSuccess("Изменения успешно сохранены!", "Успех");
        
        _parent.ShowMainMenu();
    }
    catch (Exception ex)
    {
        MessageBoxHelper.ShowError($"Ошибка при сохранении: {ex.Message}", "Ошибка");
    }
}
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _parent.ShowMainMenu();
        }

        private void GoBack()
        {
            VariantOverlay.Visibility = Visibility.Visible;
            _currentOption = null;
        }
    }
}