using System;
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

        // Динамическое изменение размера шрифта
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

        private void QueriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasRealSelection = QueriesComboBox.SelectedIndex > 0;

            DeleteQueryButton.IsEnabled = hasRealSelection;

            if (hasRealSelection)
            {
                QueryNameTextBox.Text = (QueriesComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            }
        }
        
        private void DeleteQuery_Click(object sender, RoutedEventArgs e)
        {
            if (QueriesComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var result = MessageBox.Show(
                    $"Удалить запрос:\n\n{selectedItem.Content} ?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    QueriesComboBox.Items.Remove(selectedItem);
                    QueriesComboBox.SelectedIndex = 0;
                    DeleteQueryButton.IsEnabled = false;

                    QueryNameTextBox.Clear();
                    QueryConditionTextBox.Clear();
                    QueryCodeTextBox.Clear();
                }
            }
        }

        private void NewQuery_Click(object sender, RoutedEventArgs e)
        {
            QueriesComboBox.SelectedIndex = 0;
            QueryNameTextBox.Clear();
            QueryConditionTextBox.Clear();
            QueryCodeTextBox.Clear();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Возврат к DatabaseConfigView
            FadeOutAndSwitch(() => _parent?.ShowDatabaseConfig("existing"));
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QueryNameTextBox.Text))
            {
                MessageBox.Show("Введите имя запроса!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Переход к выбору уровня
            FadeOutAndSwitch(() => _parent?.ShowLevelView());
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