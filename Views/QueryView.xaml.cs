using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Services;
using PosnaiSQLauncher.Entities;
using PosnaiSQLauncher.Models;
using PosnaiSQLauncher.Helpers;

namespace PosnaiSQLauncher
{
    public partial class QueryView : UserControl
    {
        private MainWindow _parent;
        private int _databaseId;
        private string _mode;
        
        private readonly QueryService _queryService;
        private readonly DatabaseService _databaseService;
        private Query _currentQuery;

        // Флаг разрешения редактирования
        private bool _isEditAllowed = false;

        public QueryView(MainWindow parent, int databaseId, string mode = "new")
        {
            InitializeComponent();
            _parent = parent;
            
            if (_parent.CurrentOption.DatabaseId > 0)
            {
                _databaseId = _parent.CurrentOption.DatabaseId;
            }
            else
            {
                _databaseId = databaseId;
                _parent.CurrentOption.DatabaseId = databaseId;
            }
    
            _mode = mode;

            var context = new AppDbContext();
            _queryService = new QueryService(context);
            _databaseService = new DatabaseService(context);

            RestoreFromState();
    
            // Переводим интерфейс в нужный режим (чтение или редактирование)
            ApplyInterfaceReadOnlyMode();

            if (_mode == "existing" && _currentQuery == null) 
            {
                this.Loaded += async (s, e) => await ShowQuerySelectorAsync();
            }
            else if (_mode == "new" && _currentQuery == null)
            {
                QuerySelectorSection.Visibility = Visibility.Collapsed;
                SubtitleText.Text = "Создайте новый SQL запрос";
                ClearFields();
            }
        }

        // Метод управления режимом "Только для чтения"
        private void ApplyInterfaceReadOnlyMode()
        {
            if (_mode == "existing" && _currentQuery != null)
            {
                // Блокируем текстовые поля
                QueryNameTextBox.IsReadOnly = true;
                QueryConditionTextBox.IsReadOnly = true;
                QueryCodeTextBox.IsReadOnly = true;

                // Красим фон полей в серый цвет
                var readonlyBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));
                QueryNameTextBox.Background = readonlyBg;
                QueryConditionTextBox.Background = readonlyBg;
                QueryCodeTextBox.Background = readonlyBg;
                
                // Показываем кнопку разблокировки
                ToggleEditButton.Visibility = Visibility.Visible;
                ToggleEditButton.Content = "🔒 Разрешить изменение";
                _isEditAllowed = false;
            }
            else
            {
                // Разблокируем всё (для новых запросов или если разрешили редактирование)
                QueryNameTextBox.IsReadOnly = false;
                QueryConditionTextBox.IsReadOnly = false;
                QueryCodeTextBox.IsReadOnly = false;

                var whiteBg = new SolidColorBrush(Colors.White);
                QueryNameTextBox.Background = whiteBg;
                QueryConditionTextBox.Background = whiteBg;
                QueryCodeTextBox.Background = whiteBg;
                
                ToggleEditButton.Visibility = Visibility.Collapsed;
                _isEditAllowed = true;
            }
        }

        private async Task LoadQueriesAsync()
        {
            try
            {
                var loading = new LoadingOverlay("Загрузка запросов...");
                LoadingOverlayContainer.Children.Clear();
                LoadingOverlayContainer.Children.Add(loading);
                LoadingOverlayContainer.Visibility = Visibility.Visible;

                var queries = await _queryService.GetByDatabaseIdAsync(_databaseId);
        
                LoadingOverlayContainer.Visibility = Visibility.Collapsed;

                if (queries == null || queries.Count == 0)
                {
                    HideQuerySelector();
                    MessageBoxHelper.ShowInfo(
                        "В этой базе данных нет сохраненных запросов.\n\nСоздайте новый запрос.",
                        "Запросы не найдены"
                    );
                    return;
                }

                QuerySelectorComboBox.Items.Clear();
        
                foreach (var query in queries)
                {
                    QuerySelectorComboBox.Items.Add(new QueryComboItem 
                    { 
                        Id = query.IdQuery,
                        Name = query.Name
                    });
                }

                if (QuerySelectorComboBox.Items.Count > 0)
                    QuerySelectorComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LoadingOverlayContainer.Visibility = Visibility.Collapsed;
                HideQuerySelector();
                MessageBoxHelper.ShowError($"Ошибка загрузки запросов: {ex.Message}", "Ошибка при загрузке");
            }
        }

        private async Task ShowQuerySelectorAsync()
        {
            QuerySelectorOverlay.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            QuerySelectorPanel.BeginAnimation(OpacityProperty, fadeIn);
            await LoadQueriesAsync();
        }

        private void CloseQuerySelector_Click(object sender, RoutedEventArgs e)
        {
            HideQuerySelector(true);
        }

        private void HideQuerySelector(bool isBackClick = false)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            fadeOut.Completed += (s, args) =>
            {
                QuerySelectorOverlay.Visibility = Visibility.Collapsed;

                // Если вышли через крестик, возвращаем на экран выбора режима (новый/существующий)
                if (isBackClick)
                {
                    FadeOutAndSwitch(() => _parent?.ShowQueryModeView(_databaseId));
                }
            };

            QuerySelectorPanel.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void QuerySelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isSelected = QuerySelectorComboBox.SelectedItem != null;
            SelectQueryButton.IsEnabled = isSelected;
            DeleteQueryButton.IsEnabled = isSelected;
        }

        private async void SelectQuery_Click(object sender, RoutedEventArgs e)
        {
            if (QuerySelectorComboBox.SelectedItem is not QueryComboItem selectedItem)
            {
                MessageBoxHelper.ShowWarning("Выберите запрос из списка");
                return;
            }

            try
            {
                _currentQuery = await _queryService.GetByIdAsync(selectedItem.Id);
                
                if (_currentQuery == null)
                {
                    MessageBoxHelper.ShowError(
                        $"Запрос «{selectedItem.Name}» не найден.\n\n" +
                        $"Он был удален из системы. Обновляю список...",
                        "Запрос не найден"
                    );
                    
                    await LoadQueriesAsync();
                    return;
                }
                
                QueryNameTextBox.Text = _currentQuery.Name;
                QueryConditionTextBox.Text = _currentQuery.Condition ?? "";
                QueryCodeTextBox.Text = _currentQuery.QueryString ?? "";
                SubtitleText.Text = "Настройте выбранный запрос";

                _parent.CurrentOption.QueryId = _currentQuery.IdQuery;
                _parent.CurrentOption.QueryName = _currentQuery.Name;
                _parent.CurrentOption.QueryCondition = _currentQuery.Condition;
                _parent.CurrentOption.QueryString = _currentQuery.QueryString;

                // Включаем режим только для чтения после выбора существующего запроса
                ApplyInterfaceReadOnlyMode();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка загрузкизапроса: {ex.Message}", "Ошибка при загрузке");
                return;
            }

            HideQuerySelector();
        }

        // ОБРАБОТЧИК КЛИКА КНОПКИ РАЗБЛОКИРОВКИ
        private void ToggleEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditAllowed)
            {
                var confirmBox = new CustomMessageBox(
                    "Вы уверены, что хотите разрешить редактирование полей этого запроса?\n\nИзменение условия задачи или текста кода перезапишет текущие данные.",
                    "Запрос на редактирование",
                    CustomMessageBoxType.YesNo
                );

                confirmBox.ShowDialog();

                if (confirmBox.Result == MessageBoxResult.Yes)
                {
                    _isEditAllowed = true;
                    
                    QueryNameTextBox.IsReadOnly = false;
                    QueryConditionTextBox.IsReadOnly = false;
                    QueryCodeTextBox.IsReadOnly = false;

                    var whiteBg = new SolidColorBrush(Colors.White);
                    QueryNameTextBox.Background = whiteBg;
                    QueryConditionTextBox.Background = whiteBg;
                    QueryCodeTextBox.Background = whiteBg;
                    
                    ToggleEditButton.Content = "🔓 Изменение разрешено";
                    QueryNameTextBox.Focus();
                }
            }
            else
            {
                ApplyInterfaceReadOnlyMode();
            }
        }

        private async void DeleteQuery_Click(object sender, RoutedEventArgs e)
        {
            if (QuerySelectorComboBox.SelectedItem is not QueryComboItem selectedItem)
                return;

            var result = MessageBoxHelper.ShowDeleteConfirmation(selectedItem.Name);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _queryService.DeleteAsync(selectedItem.Id);
                    await LoadQueriesAsync();
                    MessageBoxHelper.ShowSuccess("Запрос успешно удален");
                }
                catch (Exception ex)
                {
                    MessageBoxHelper.ShowError($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                int textLength = textBox.Text.Length;
                if (textLength < 100) textBox.FontSize = 16;
                else if (textLength < 300) textBox.FontSize = 14;
                else if (textLength < 600) textBox.FontSize = 12;
                else textBox.FontSize = 11;
            }
        }

        private void ClearFields()
        {
            QueryNameTextBox.Clear();
            QueryConditionTextBox.Clear();
            QueryCodeTextBox.Clear();
            _currentQuery = null;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent?.ShowQueryModeView(_databaseId));
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QueryNameTextBox.Text))
            {
                MessageBoxHelper.ShowWarning("Введите имя запроса!");
                return;
            }

            try
            {
                if (_currentQuery == null)
                {
                    _currentQuery = await _queryService.CreateAsync(
                        _databaseId,
                        QueryNameTextBox.Text.Trim(),
                        QueryConditionTextBox.Text.Trim(),
                        QueryCodeTextBox.Text.Trim()
                    );
                }
                else
                {
                    _currentQuery = await _queryService.UpdateAsync(
                        _currentQuery.IdQuery,
                        QueryNameTextBox.Text.Trim(),
                        QueryConditionTextBox.Text.Trim(),
                        QueryCodeTextBox.Text.Trim()
                    );
                }

                _parent.CurrentOption.QueryMode = _mode; 
                _parent.CurrentOption.QueryId = _currentQuery.IdQuery;
                _parent.CurrentOption.QueryName = _currentQuery.Name;
                _parent.CurrentOption.QueryCondition = _currentQuery.Condition;
                _parent.CurrentOption.QueryString = _currentQuery.QueryString;

                FadeOutAndSwitch(() => _parent?.ShowLevelView(_currentQuery.IdQuery));
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }
        
        private void FadeOutAndSwitch(Action switchAction)
        {
            var fadeOut = new DoubleAnimation 
            { 
                From = 1, 
                To = 0, 
                Duration = TimeSpan.FromMilliseconds(300) 
            };
            
            var storyboard = new Storyboard();
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOut);
            storyboard.Completed += (s, e) => { switchAction(); };
            storyboard.Begin(this);
        }
        
        private void RestoreFromState()
        {
            var state = _parent.CurrentOption;

            if (state.QueryId > 0)
            {
                QueryNameTextBox.Text = state.QueryName;
                QueryConditionTextBox.Text = state.QueryCondition;
                QueryCodeTextBox.Text = state.QueryString;

                _currentQuery = new Query
                {
                    IdQuery = state.QueryId,
                    Name = state.QueryName,
                    Condition = state.QueryCondition,
                    QueryString = state.QueryString,
                    IdDatabase = state.DatabaseId
                };

                SubtitleText.Text = "Отредактируйте выбранный запрос";
            }
        }
    }
}