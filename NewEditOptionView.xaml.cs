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
            
            // Изначально подсвечиваем пустыню как активную по умолчанию (опционально)
            AnimateBorder(CardDesert, (Color)ColorConverter.ConvertFromString("#2196F3"), 3);
        }

        private void VariantComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = VariantComboBox.SelectedItem != null;
            OpenVariantButton.IsEnabled = hasSelection;
            DeleteVariantButton.IsEnabled = hasSelection;
        }
        
        private void DeleteVariant_Click(object sender, RoutedEventArgs e)
        {
            if (VariantComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var result = MessageBox.Show(
                    $"Удалить вариант:\n\n{selectedItem.Content} ?",
                    "Удаление",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    VariantComboBox.Items.Remove(selectedItem);
                    OpenVariantButton.IsEnabled = false;
                    DeleteVariantButton.IsEnabled = false;
                }
            }
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
        
        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteDatabaseButton.IsEnabled = DatabaseComboBox.SelectedIndex > 0;
        }

        private void DeleteDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (DatabaseComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var result = MessageBox.Show(
                    $"Удалить базу данных:\n\n{selectedItem.Content} ?",
                    "Удаление",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DatabaseComboBox.Items.Remove(selectedItem);
                    DeleteDatabaseButton.IsEnabled = false;
                }
            }
        }
        
        private void QueryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteQueryButton.IsEnabled = QueryComboBox.SelectedIndex > 0;
        }

        private void DeleteQuery_Click(object sender, RoutedEventArgs e)
        {
            if (QueryComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var result = MessageBox.Show(
                    $"Удалить запрос:\n\n{selectedItem.Content} ?",
                    "Удаление",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    QueryComboBox.Items.Remove(selectedItem);
                    DeleteQueryButton.IsEnabled = false;
                }
            }
        }

        // ===== ТАЙМЕР =====
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

        // ===== КАРТОЧКИ ЛОКАЦИЙ (ПЛАВНАЯ АНИМАЦИЯ) =====
        private void CardDesert_Click(object sender, MouseButtonEventArgs e)
        {
            AnimateBorder(CardDesert, (Color)ColorConverter.ConvertFromString("#2196F3"), 3);
            AnimateBorder(CardForest, (Color)ColorConverter.ConvertFromString("#E0E0E0"), 2);
        }

        private void CardForest_Click(object sender, MouseButtonEventArgs e)
        {
            AnimateBorder(CardForest, (Color)ColorConverter.ConvertFromString("#2196F3"), 3);
            AnimateBorder(CardDesert, (Color)ColorConverter.ConvertFromString("#E0E0E0"), 2);
        }

        private void AnimateBorder(Border border, Color targetColor, double targetThickness)
        {
            ColorAnimation colorAnim = new ColorAnimation { To = targetColor, Duration = TimeSpan.FromMilliseconds(300) };
            ThicknessAnimation thickAnim = new ThicknessAnimation { To = new Thickness(targetThickness), Duration = TimeSpan.FromMilliseconds(300) };

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

        // ===== ФАЙЛЫ И НАВИГАЦИЯ =====
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
                try
                {
                    ImagePreview.Source = new BitmapImage(new Uri(_selectedImagePath));
                    ImagePlaceholder.Visibility = Visibility.Collapsed;
                }
                catch
                {
                    MessageBox.Show("Не удалось загрузить изображение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Изменения успешно сохранены!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
            Back_Click(sender, e);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, ev) => _parent.ShowMainMenu();
            this.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}