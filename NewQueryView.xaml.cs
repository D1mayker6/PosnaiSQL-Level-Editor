using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace PosnaiSQLauncher
{
    public partial class NewQueryView : UserControl
    {
        private NewMainWindow _parent;

        public NewQueryView(NewMainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
        }

        private void QueriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // При выборе запроса из выпадашки загружаем его данные
            if (QueriesComboBox.SelectedIndex > 0)
            {
                // TODO: Загрузить данные запроса с бэка
                QueryNameTextBox.Text = $"Запрос {QueriesComboBox.SelectedIndex}";
                QueryConditionTextBox.Text = "id > 10";
                QueryCodeTextBox.Text = "SELECT * FROM users WHERE id > 10";
            }
        }

        private void NewQuery_Click(object sender, RoutedEventArgs e)
        {
            // Очищаем все поля для нового запроса
            QueriesComboBox.SelectedIndex = 0;
            QueryNameTextBox.Clear();
            QueryConditionTextBox.Clear();
            QueryCodeTextBox.Clear();
            QueryNameTextBox.Focus();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent?.ShowDatabaseConfig("create")); // или "existing"
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QueryNameTextBox.Text))
            {
                MessageBox.Show("Введите имя запроса", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(QueryCodeTextBox.Text))
            {
                MessageBox.Show("Введите SQL запрос", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Переход на следующий шаг (Уровень)
            MessageBox.Show($"Запрос: {QueryNameTextBox.Text}", "Данные сохранены");
        }

        private void FadeOutAndSwitch(System.Action switchAction)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = System.TimeSpan.FromMilliseconds(300)
            };

            var storyboard = new Storyboard();
            Storyboard.SetTargetProperty(fadeOut, new System.Windows.PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOut);

            storyboard.Completed += (s, e) => { switchAction(); };

            storyboard.Begin(this);
        }
    }
}