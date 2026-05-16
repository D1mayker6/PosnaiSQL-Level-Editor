using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        public QueryView(MainWindow parent, int databaseId, string mode = "new")
        {
            InitializeComponent();
            _parent = parent;
            _databaseId = databaseId;
            _mode = mode;

            var context = new AppDbContext();
            _queryService = new QueryService(context);
            _databaseService = new DatabaseService(context);

            if (_mode == "existing")
            {
                this.Loaded += (s, e) => ShowQuerySelector();
            }
            else
            {
                QuerySelectorSection.Visibility = Visibility.Collapsed;
                SubtitleText.Text = "Создайте новый SQL запрос";
                ClearFields();
            }
        }

        private async void LoadQueriesAsync()
        {
            try
            {
                // ← ПОКАЗЫВАЕМ загрузку
                this.Dispatcher.Invoke(() =>
                {
                    var loading = new LoadingOverlay("Загрузка запросов...");
                    LoadingOverlayContainer.Children.Clear();
                    LoadingOverlayContainer.Children.Add(loading);
                    LoadingOverlayContainer.Visibility = Visibility.Visible;
                });

                var queries = await _queryService.GetByDatabaseIdAsync(_databaseId);
        
                this.Dispatcher.Invoke(() =>
                {
                    // ← СКРЫВАЕМ загрузку
                    LoadingOverlayContainer.Visibility = Visibility.Collapsed;

                    if (queries == null || queries.Count == 0)
                    {
                        HideQuerySelector();
                        MessageBoxHelper.ShowInfo(
                            "В этой базе данных нет сохраненных запросов.\n\n" +
                            "Создайте новый запрос.",
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
                });
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(() =>
                {
                    LoadingOverlayContainer.Visibility = Visibility.Collapsed;
                    HideQuerySelector();
                    MessageBoxHelper.ShowError(
                        $"Ошибка загрузки запросов: {ex.Message}",
                        "Ошибка при загрузке"
                    );
                });
            }
        }
        private void ShowQuerySelector()
        {
            QuerySelectorOverlay.Visibility = Visibility.Visible;

            System.Threading.Tasks.Task.Run(() => LoadQueriesAsync());

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            QuerySelectorPanel.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void CloseQuerySelector_Click(object sender, RoutedEventArgs e)
        {
            HideQuerySelector();
        }

        private void HideQuerySelector()
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
                    
                    System.Threading.Tasks.Task.Run(() => LoadQueriesAsync());
                    return;
                }
                
                QueryNameTextBox.Text = _currentQuery.Name;
                QueryConditionTextBox.Text = _currentQuery.Condition ?? "";
                QueryCodeTextBox.Text = _currentQuery.QueryString ?? "";
                SubtitleText.Text = "Отредактируйте выбранный запрос";

                _parent.CurrentOption.QueryId = _currentQuery.IdQuery;
                _parent.CurrentOption.QueryName = _currentQuery.Name;
                _parent.CurrentOption.QueryCondition = _currentQuery.Condition;
                _parent.CurrentOption.QueryString = _currentQuery.QueryString;
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(
                    $"Ошибка загрузки запроса: {ex.Message}",
                    "Ошибка при загрузке"
                );
                return;
            }

            HideQuerySelector();
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
                    System.Threading.Tasks.Task.Run(() => LoadQueriesAsync());
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
    }
}