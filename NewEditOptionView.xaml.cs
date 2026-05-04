using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace PosnaiSQLauncher
{
    public partial class NewEditOptionView : UserControl
    {
        private NewMainWindow _parent;
        private int _totalSeconds = 300;
        private string _selectedImagePath;

        public NewEditOptionView(NewMainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            UpdateTimeDisplay();
        }

        private void VariantComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OpenVariantButton.IsEnabled = VariantComboBox.SelectedItem != null;
        }

        private void OpenVariant_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s, ev) => {
                VariantOverlay.Visibility = Visibility.Collapsed;
                MainBorder.Opacity = 1;
                MainBorder.IsHitTestVisible = true;
            };
            VariantOverlay.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                int len = tb.Text.Length;
                if (len < 100) tb.FontSize = 16;
                else if (len < 300) tb.FontSize = 14;
                else tb.FontSize = 12;
            }
        }

        private void TimePlus5_Click(object sender, RoutedEventArgs e) => AddTime(5);
        private void TimePlus30_Click(object sender, RoutedEventArgs e) => AddTime(30);
        private void TimeMinus5_Click(object sender, RoutedEventArgs e) => AddTime(-5);
        private void TimeMinus30_Click(object sender, RoutedEventArgs e) => AddTime(-30);

        private void AddTime(int s)
        {
            _totalSeconds = Math.Max(0, Math.Min(3595, _totalSeconds + s));
            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay()
        {
            MinDisplay.Text = (_totalSeconds / 60).ToString("D2");
            SecDisplay.Text = (_totalSeconds % 60).ToString("D2");
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, ev) => _parent.ShowMainMenu();
            this.BeginAnimation(OpacityProperty, fadeOut);
        }
        
        private void CardDesert_Click(object sender, MouseButtonEventArgs e)
        {
            CardDesert.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
            CardForest.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
        }

        private void CardForest_Click(object sender, MouseButtonEventArgs e)
        {
            CardForest.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
            CardDesert.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
        }
        
        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp|Все файлы|*.*",
                Title = "Выберите изображение базы данных"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;

                // Показываем превью
                try
                {
                    var bitmap = new BitmapImage(new Uri(_selectedImagePath));
                    ImagePreview.Source = bitmap;
                    ImagePlaceholder.Visibility = Visibility.Collapsed;
                }
                catch
                {
                    MessageBox.Show("Не удалось загрузить изображение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}