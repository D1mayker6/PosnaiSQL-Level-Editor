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
        private string _selectedFormat = null;

        public NewExportView(NewMainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
        }

        private void CardJson_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectFormat(CardJson, CardExcel, "json");
        }

        private void CardExcel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectFormat(CardExcel, CardJson, "excel");
        }

        private void SelectFormat(Border selectedCard, Border otherCard, string format)
        {
            _selectedFormat = format;
            CheckReadyToExport();
            AnimateBorder(selectedCard, new SolidColorBrush(Color.FromRgb(33, 150, 243)), 3);
            AnimateBorder(otherCard, new SolidColorBrush(Color.FromRgb(224, 224, 224)), 2);
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (_selectedFormat == "json")
            {
                saveFileDialog.Filter = "JSON files (*.json)|*.json";
                saveFileDialog.FileName = "task_export.json";
            }
            else
            {
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                saveFileDialog.FileName = "task_export.xlsx";
            }

            if (saveFileDialog.ShowDialog() == true)
            {
                PathTextBox.Text = saveFileDialog.FileName;
                CheckReadyToExport();
            }
        }

        private void CheckReadyToExport()
        {
            ExportButton.IsEnabled = !string.IsNullOrEmpty(_selectedFormat) && !string.IsNullOrEmpty(PathTextBox.Text);
        }

        private void AnimateBorder(Border border, SolidColorBrush targetColor, double thickness)
        {
            var colorAnim = new ColorAnimation { To = targetColor.Color, Duration = TimeSpan.FromMilliseconds(250) };
            var thickAnim = new ThicknessAnimation { To = new Thickness(thickness), Duration = TimeSpan.FromMilliseconds(250) };
            var brush = border.BorderBrush is SolidColorBrush b ? (b.IsFrozen ? b.Clone() : b) : targetColor;
            border.BorderBrush = brush;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            border.BeginAnimation(Border.BorderThicknessProperty, thickAnim);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent?.ShowLevelView());
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Данные успешно сохранены по пути:\n{PathTextBox.Text}", "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
            FadeOutAndSwitch(() => _parent?.ShowMainMenu());
        }

        private void FadeOutAndSwitch(Action switchAction)
        {
            var fadeOut = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(300) };
            var storyboard = new Storyboard();
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOut);
            storyboard.Completed += (s, e) => switchAction();
            storyboard.Begin(this);
        }
    }
}