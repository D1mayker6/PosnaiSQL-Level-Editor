using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace PosnaiSQLauncher
{
    public partial class DatabaseConfigView : UserControl
    {
        private NewMainWindow _parent;
        private string _selectedOption;
        private string _selectedImagePath;

        public DatabaseConfigView(NewMainWindow parent, string option)
        {
            InitializeComponent();
            _parent = parent;
            _selectedOption = option;

            // Если выбрана существующая БД - показываем оверлей с выбором
            if (option == "existing")
            {
                this.Loaded += (s, e) => ShowDatabaseSelector();
            }
        }

        private void ShowDatabaseSelector()
        {
            // Затемняем основной контент
            MainBorder.Opacity = 0.3;
            MainBorder.IsHitTestVisible = false;

            // Показываем оверлей
            DatabaseSelectorOverlay.Visibility = Visibility.Visible;

            // Анимация появления панели
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            DatabaseSelectorPanel.BeginAnimation(OpacityProperty, fadeIn);
        }

// Обновляем метод выбора: включаем сразу обе кнопки
        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isSelected = DatabaseComboBox.SelectedItem != null;
    
            if (SelectDatabaseButton != null) 
                SelectDatabaseButton.IsEnabled = isSelected;
        
            if (DeleteDatabaseButton != null) 
                DeleteDatabaseButton.IsEnabled = isSelected;
        }

// НОВЫЙ МЕТОД: Обработка удаления
        private void DeleteDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (DatabaseComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string dbName = selectedItem.Content.ToString();

                // Защитный вопрос
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите безвозвратно удалить базу данных «{dbName}»?",
                    "Удаление базы данных",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Удаляем итем из UI
                    DatabaseComboBox.Items.Remove(selectedItem);
            
                    // Сбрасываем выбор
                    DatabaseComboBox.SelectedItem = null;
            
                    MessageBox.Show($"База данных «{dbName}» успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Если список баз опустел, можно автоматически вернуть пользователя назад
                    if (DatabaseComboBox.Items.Count == 0)
                    {
                        MessageBox.Show("Список баз данных пуст. Создайте новую базу.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        Back_Click(sender, e);
                    }
                }
            }
        }

        private void SelectDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (DatabaseComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                DatabaseNameTextBox.Text = selectedItem.Content.ToString().Replace(".db", "");
                SubtitleText.Text = "Настройте выбранную базу данных";
            }

            // Анимация исчезновения панели
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fadeOut.Completed += (s, args) =>
            {
                DatabaseSelectorOverlay.Visibility = Visibility.Collapsed;
                MainBorder.Opacity = 1;
                MainBorder.IsHitTestVisible = true;
            };

            DatabaseSelectorPanel.BeginAnimation(OpacityProperty, fadeOut);
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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent?.ShowCreateOption());
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(DatabaseNameTextBox.Text))
            {
                MessageBox.Show("Введите имя базы данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FadeOutAndSwitch(() => _parent.ShowQueryView());        }

        private void FadeOutAndSwitch(Action switchAction)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            var storyboard = new Storyboard();
            Storyboard.SetTargetProperty(fadeOut, new System.Windows.PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOut);

            storyboard.Completed += (s, e) => { switchAction(); };

            storyboard.Begin(this);
        }
        
        
    }
}