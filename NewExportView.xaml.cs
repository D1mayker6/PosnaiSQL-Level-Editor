using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;

namespace PosnaiSQLauncher
{
    public partial class NewExportView : UserControl
    {
        private NewMainWindow _parent;
        private string _selectedFormat = "";

        public NewExportView(NewMainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
        }

        private void CardJson_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _selectedFormat = "json";
            HighlightCard(CardJson, CardExcel, "#E65100");
        }

        private void CardExcel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _selectedFormat = "excel";
            HighlightCard(CardExcel, CardJson, "#2E7D32");
        }

        private void HighlightCard(Border selected, Border other, string colorHex)
        {
            var activeColor = (Color)ColorConverter.ConvertFromString(colorHex);
            var inactiveColor = (Color)ColorConverter.ConvertFromString("#E0E0E0");

            // Анимируем выбранную карточку
            AnimateBorder(selected, activeColor, 3);
            // Возвращаем вторую карточку в исходное состояние
            AnimateBorder(other, inactiveColor, 2);

            ValidateExport();
        }

        private void AnimateBorder(Border border, Color targetColor, double targetThickness)
        {
            // Анимация цвета
            ColorAnimation colorAnim = new ColorAnimation
            {
                To = targetColor,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Анимация толщины
            ThicknessAnimation thickAnim = new ThicknessAnimation
            {
                To = new Thickness(targetThickness),
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Применяем к обводке
            if (border.BorderBrush is SolidColorBrush currentBrush)
            {
                if (currentBrush.IsFrozen) border.BorderBrush = currentBrush.Clone();
                border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }
            else
            {
                border.BorderBrush = new SolidColorBrush(Colors.Transparent);
                border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }

            border.BeginAnimation(Border.BorderThicknessProperty, thickAnim);
        }

        // Остальные методы (Browse, Search, Back, Export) без изменений...
        private void VariantsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SelectedCountText.Text = $"Выбрано: {VariantsListBox.SelectedItems.Count}";
            ValidateExport();
        }

        private void ValidateExport() {
            ExportButton.IsEnabled = VariantsListBox.SelectedItems.Count > 0 && !string.IsNullOrEmpty(_selectedFormat) && !string.IsNullOrEmpty(PathTextBox.Text);
        }

        private void Browse_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog dlg = new SaveFileDialog { Filter = _selectedFormat == "excel" ? "Excel|*.xlsx" : "JSON|*.json" };
            if (dlg.ShowDialog() == true) { PathTextBox.Text = dlg.FileName; ValidateExport(); }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) { /* Фильтрация */ }

        private void Export_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("Готово!", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
            _parent.ShowMainMenu();
        }

        private void Back_Click(object sender, RoutedEventArgs e) {
            var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            anim.Completed += (s, ev) => _parent.ShowMainMenu();
            this.BeginAnimation(OpacityProperty, anim);
        }
    }
}