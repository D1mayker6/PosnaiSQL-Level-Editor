// NewQueryView.xaml.cs
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Services;
using PosnaiSQLauncher.Entities;

namespace PosnaiSQLauncher
{
    public partial class QueryView : UserControl
    {
        private MainWindow _parent;
        private int _databaseId;
        
        private readonly QueryService _queryService;
        private readonly DatabaseService _databaseService;
        private Query _currentQuery;

        public QueryView(MainWindow parent, int databaseId)
        {
            InitializeComponent();
            _parent = parent;
            _databaseId = databaseId;

            // Инициализируем сервисы
            var context = new AppDbContext();
            _queryService = new QueryService(context);
            _databaseService = new DatabaseService(context);

            // Загружаем запросы для текущей БД
            LoadQueries();
        }

        /// <summary>
        /// Загружает список запросов для выбранной БД
        /// </summary>
        private async void LoadQueries()
        {
            try
            {
                QueriesComboBox.Items.Clear();
                
                // Добавляем "Новый запрос"
                QueriesComboBox.Items.Add(new ComboBoxItem 
                { 
                    Content = "Новый запрос (пустой)",
                    Tag = 0
                });

                // Загружаем существующие запросы
                var queries = await _queryService.GetByDatabaseIdAsync(_databaseId);
                
                foreach (var query in queries)
                {
                    QueriesComboBox.Items.Add(new ComboBoxItem 
                    { 
                        Content = query.Name,
                        Tag = query.IdQuery
                    });
                }

                // Выбираем первый элемент по умолчанию
                QueriesComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки запросов: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        /// <summary>
        /// Обработка выбора запроса из ComboBox
        /// </summary>
        private async void QueriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QueriesComboBox.SelectedItem is not ComboBoxItem selectedItem)
                return;

            int queryId = (int)selectedItem.Tag;
            bool hasRealSelection = queryId > 0;

            DeleteQueryButton.IsEnabled = hasRealSelection;

            if (hasRealSelection)
            {
                try
                {
                    _currentQuery = await _queryService.GetByIdAsync(queryId);
                    
                    // Заполняем поля
                    QueryNameTextBox.Text = _currentQuery.Name;
                    QueryConditionTextBox.Text = _currentQuery.Condition ?? "";
                    QueryCodeTextBox.Text = _currentQuery.QueryString ?? "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки запроса: {ex.Message}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _currentQuery = null;
                ClearFields();
            }
        }

        /// <summary>
        /// Очистка всех полей
        /// </summary>
        private void ClearFields()
        {
            QueryNameTextBox.Clear();
            QueryConditionTextBox.Clear();
            QueryCodeTextBox.Clear();
        }

        /// <summary>
        /// Удаление запроса
        /// </summary>
        private async void DeleteQuery_Click(object sender, RoutedEventArgs e)
        {
            if (QueriesComboBox.SelectedItem is not ComboBoxItem selectedItem || _currentQuery == null)
                return;

            var result = MessageBox.Show(
                $"Удалить запрос:\n\n{selectedItem.Content} ?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _queryService.DeleteAsync(_currentQuery.IdQuery);
                    
                    QueriesComboBox.Items.Remove(selectedItem);
                    QueriesComboBox.SelectedIndex = 0;
                    DeleteQueryButton.IsEnabled = false;

                    ClearFields();

                    MessageBox.Show("Запрос успешно удален", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Создание нового пустого запроса
        /// </summary>
        private void NewQuery_Click(object sender, RoutedEventArgs e)
        {
            QueriesComboBox.SelectedIndex = 0;
            ClearFields();
            QueryNameTextBox.Focus();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent?.ShowDatabaseConfig("existing"));
        }

        /// <summary>
        /// Переход на следующий шаг (сохраняем запрос)
        /// </summary>
        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QueryNameTextBox.Text))
            {
                MessageBox.Show("Введите имя запроса!", 
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Если создаем новый запрос
                if (_currentQuery == null)
                {
                    _currentQuery = await _queryService.CreateAsync(
                        _databaseId,
                        QueryNameTextBox.Text.Trim(),
                        QueryConditionTextBox.Text.Trim(),
                        QueryCodeTextBox.Text.Trim()
                    );
                }
                // Если редактируем существующий
                else
                {
                    _currentQuery = await _queryService.UpdateAsync(
                        _currentQuery.IdQuery,
                        QueryNameTextBox.Text.Trim(),
                        QueryConditionTextBox.Text.Trim(),
                        QueryCodeTextBox.Text.Trim()
                    );
                }

                // Переход к выбору уровня (передаем ID запроса)
                FadeOutAndSwitch(() => _parent?.ShowLevelView(_currentQuery.IdQuery));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FadeOutAndSwitch(Action switchAction)
        {
            var fadeOut = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(300) };
            var storyboard = new Storyboard();
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOut);
            storyboard.Completed += (s, e) => { switchAction(); };
            storyboard.Begin(this);
        }
    }
}