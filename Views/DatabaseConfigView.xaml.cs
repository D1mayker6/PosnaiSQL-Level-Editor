using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Services;
using PosnaiSQLauncher.Entities;
using PosnaiSQLauncher.Models;

namespace PosnaiSQLauncher
{
    public partial class DatabaseConfigView : UserControl
    {
        private MainWindow _parent;
        private string _selectedOption;
        private string _selectedImagePath;
        
        private readonly DatabaseService _databaseService;
        private Database _currentDatabase;

        public DatabaseConfigView(MainWindow parent, string option)
        {
            InitializeComponent();
            _parent = parent;
            _selectedOption = option;

            var context = new AppDbContext();
            _databaseService = new DatabaseService(context);

            LoadDatabases();

            if (option == "existing")
            {
                this.Loaded += (s, e) => ShowDatabaseSelector();
            }
        }

        /// <summary>
        /// Загружает список баз данных в ComboBox
        /// </summary>
        private async void LoadDatabases()
        {
            try
            {
                DatabaseComboBox.Items.Clear();
                
                var databases = await _databaseService.GetAllAsync();
                
                foreach (var db in databases)
                {
                    DatabaseComboBox.Items.Add(new DatabaseComboItem 
                    { 
                        Id = db.IdDatabase,
                        Name = db.Name
                    });
                }

                // Выбираем первый элемент если есть
                if (DatabaseComboBox.Items.Count > 0)
                    DatabaseComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки баз данных: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowDatabaseSelector()
        {
            MainBorder.Opacity = 0.3;
            MainBorder.IsHitTestVisible = false;
            DatabaseSelectorOverlay.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            DatabaseSelectorPanel.BeginAnimation(OpacityProperty, fadeIn);
        }

        /// <summary>
        /// Обработка выбора БД из ComboBox
        /// </summary>
        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isSelected = DatabaseComboBox.SelectedItem != null;
    
            SelectDatabaseButton.IsEnabled = isSelected;
            DeleteDatabaseButton.IsEnabled = isSelected;
        }

        /// <summary>
        /// Удаление базы данных
        /// </summary>
        private async void DeleteDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (DatabaseComboBox.SelectedItem is not DatabaseComboItem selectedItem)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите безвозвратно удалить базу данных «{selectedItem.Name}»?",
                "Удаление базы данных",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _databaseService.DeleteAsync(selectedItem.Id);
                    LoadDatabases();
                    
                    MessageBox.Show($"База данных «{selectedItem.Name}» успешно удалена.", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (DatabaseComboBox.Items.Count == 0)
                    {
                        MessageBox.Show("Список баз данных пуст. Создайте новую базу.", 
                            "Внимание", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        Back_Click(sender, e);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Выбор существующей базы данных
        /// </summary>
        private async void SelectDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (DatabaseComboBox.SelectedItem is not DatabaseComboItem selectedItem)
                return;

            try
            {
                _currentDatabase = await _databaseService.GetByIdAsync(selectedItem.Id);
                DatabaseNameTextBox.Text = _currentDatabase.Name;
                SubtitleText.Text = "Настройте выбранную базу данных";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки БД: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Скрываем оверлей
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

                try
                {
                    var bitmap = new BitmapImage(new Uri(_selectedImagePath));
                    ImagePreview.Source = bitmap;
                    ImagePlaceholder.Visibility = Visibility.Collapsed;
                }
                catch
                {
                    MessageBox.Show("Не удалось загрузить изображение", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent?.ShowCreateOption());
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DatabaseNameTextBox.Text))
            {
                MessageBox.Show("Введите имя базы данных", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_selectedOption == "create" || _currentDatabase == null)
                {
                    _currentDatabase = await _databaseService.CreateAsync(
                        DatabaseNameTextBox.Text.Trim()
                    );
                }
                else
                {
                    _currentDatabase = await _databaseService.UpdateAsync(
                        _currentDatabase.IdDatabase,
                        DatabaseNameTextBox.Text.Trim()
                    );
                }

                FadeOutAndSwitch(() => _parent.ShowQueryView(_currentDatabase.IdDatabase));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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