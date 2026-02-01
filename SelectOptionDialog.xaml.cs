using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PosnaiSQLauncher.Entities;

namespace PosnaiSQLauncher
{
    public partial class SelectOptionDialog : Window
    {
        private readonly List<Option> _options;
        public List<Option> SelectedOptions { get; private set; } = new();

        public SelectOptionDialog(List<Option> options)
        {
            InitializeComponent();
            _options = options;
            RenderOptions(_options);
        }

        private void RenderOptions(IEnumerable<Option> options)
        {
            OptionsPanel.Children.Clear();
            foreach (var option in options)
            {
                var cb = new CheckBox
                {
                    Content = $"Вариант {option.IdOption}",
                    Tag = option,
                    Margin = new Thickness(4),
                    FontSize = 16
                };
                OptionsPanel.Children.Add(cb);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = SearchBox.Text;
            if (string.IsNullOrWhiteSpace(text))
                RenderOptions(_options);
            else
            {
                var filtered = _options
                    .Where(o => o.IdOption.ToString().Contains(text))
                    .ToList();
                RenderOptions(filtered);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {

            var selected = OptionsPanel.Children
                .OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .Select(cb => (Option)cb.Tag)
                .ToList();

            if (selected.Count >= 1 && selected.Count <= 5)
            {
                SelectedOptions = selected;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Выберите от 1 до 5 вариантов.",
                    "Ошибка выбора",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}
