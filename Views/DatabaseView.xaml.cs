using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Services;
using PosnaiSQLauncher.Entities;
using PosnaiSQLauncher.Models;
using PosnaiSQLauncher.Helpers;

namespace PosnaiSQLauncher
{
    public partial class DatabaseView : UserControl
    {
        private MainWindow _parent;
        private string _selectedOption;
        private string _selectedImagePath;
        
        private readonly DatabaseService _databaseService;
        private Database _currentDatabase;

        private bool _isEditAllowed = false;

        public DatabaseView(MainWindow parent, string option)
        {
            InitializeComponent();
            _parent = parent;
            _selectedOption = option;

            var context = new AppDbContext();
            _databaseService = new DatabaseService(context);

            RestoreFromState(); 

            if (_parent.CurrentOption != null && _parent.CurrentOption.DatabaseId > 0)
            {
                _selectedOption = "existing";
            }

            ApplyInterfaceReadOnlyMode();

            if (_selectedOption == "existing" && _currentDatabase == null)
            {
                this.Loaded += (s, e) => ShowDatabaseSelector();
            }
        }

        private void ApplyInterfaceReadOnlyMode()
        {
            if (_selectedOption == "existing")
            {
                DatabaseNameTextBox.IsReadOnly = true;
                DatabaseNameTextBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));
                BrowseImageButton.IsEnabled = false; 
                
                ToggleEditButton.Visibility = Visibility.Visible;
                ToggleEditButton.Content = "🔒 Разрешить изменение";
                _isEditAllowed = false;
            }
            else
            {
                DatabaseNameTextBox.IsReadOnly = false;
                DatabaseNameTextBox.Background = new SolidColorBrush(Colors.White);
                BrowseImageButton.IsEnabled = true;
                
                ToggleEditButton.Visibility = Visibility.Collapsed;
                _isEditAllowed = true;
            }
        }

        private async Task LoadDatabasesAsync()
        {
            try
            {
                LoadingOverlayContainer.Children.Clear();
                LoadingOverlayContainer.Children.Add(new LoadingOverlay("Загрузка баз данных..."));
                LoadingOverlayContainer.Visibility = Visibility.Visible;

                var databases = await _databaseService.GetAllAsync();
        
                LoadingOverlayContainer.Visibility = Visibility.Collapsed;

                DatabaseComboBox.Items.Clear();
        
                foreach (var db in databases)
                {
                    DatabaseComboBox.Items.Add(new DatabaseComboItem 
                    { 
                        Id = db.IdDatabase,
                        Name = db.Name
                    });
                }

                if (DatabaseComboBox.Items.Count > 0)
                    DatabaseComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LoadingOverlayContainer.Visibility = Visibility.Collapsed;
                MessageBoxHelper.ShowError($"Ошибка загрузки баз данных: {ex.Message}");
            }
        }

        private void ShowDatabaseSelector()
        {
            DatabaseSelectorOverlay.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            DatabaseSelectorPanel.BeginAnimation(OpacityProperty, fadeIn);
            _ = LoadDatabasesAsync();
        }

        private void CloseDatabaseSelector_Click(object sender, RoutedEventArgs e)
        {
            HideDatabaseSelector(true);
        }

        private void HideDatabaseSelector(bool isBackClick = false)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            fadeOut.Completed += (s, args) =>
            {
                DatabaseSelectorOverlay.Visibility = Visibility.Collapsed;

                if (isBackClick)
                {
                    FadeOutAndSwitch(() => _parent?.ShowDatabaseModeView()); 
                }
            };

            DatabaseSelectorPanel.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isSelected = DatabaseComboBox.SelectedItem != null;
            SelectDatabaseButton.IsEnabled = isSelected;
            DeleteDatabaseButton.IsEnabled = isSelected;
        }

        private async void DeleteDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (DatabaseComboBox.SelectedItem is not DatabaseComboItem selectedItem)
                return;

            var result = MessageBoxHelper.ShowDeleteConfirmation(selectedItem.Name);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var loading = new LoadingOverlay("Удаление базы данных...");
                    LoadingOverlayContainer.Children.Clear();
                    LoadingOverlayContainer.Children.Add(loading);
                    LoadingOverlayContainer.Visibility = Visibility.Visible;

                    await _databaseService.DeleteAsync(selectedItem.Id);
            
                    LoadingOverlayContainer.Visibility = Visibility.Collapsed;
            
                    await LoadDatabasesAsync();
            
                    MessageBoxHelper.ShowSuccess($"База данных «{selectedItem.Name}» успешно удалена");

                    if (DatabaseComboBox.Items.Count == 0)
                    {
                        MessageBoxHelper.ShowInfo("Список баз данных пуст. Создайте новую базу.");
                    }
                }
                catch (Exception ex)
                {
                    LoadingOverlayContainer.Visibility = Visibility.Collapsed;
                    MessageBoxHelper.ShowError($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private async void SelectDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (DatabaseComboBox.SelectedItem is not DatabaseComboItem selectedItem)
            {
                MessageBoxHelper.ShowWarning("Выберите базу данных из списка");
                return;
            }

            try
            {
                var loading = new LoadingOverlay("Загрузка базы данных...");
                LoadingOverlayContainer.Children.Clear();
                LoadingOverlayContainer.Children.Add(loading);
                LoadingOverlayContainer.Visibility = Visibility.Visible;

                _currentDatabase = await _databaseService.GetByIdAsync(selectedItem.Id);
        
                LoadingOverlayContainer.Visibility = Visibility.Collapsed;

                if (_currentDatabase == null)
                {
                    MessageBoxHelper.ShowError(
                        $"База данных «{selectedItem.Name}» не найдена.\n\n" +
                        $"Она была удалена из системы. Обновляю список...",
                        "База данных не найдена"
                    );
            
                    await LoadDatabasesAsync();
                    return;
                }

                DatabaseNameTextBox.Text = _currentDatabase.Name;
                SubtitleText.Text = "Настройте выбранную базу данных";

                if (!string.IsNullOrEmpty(_currentDatabase.SchemaImage))
                {
                    ImagePreview.Source = ImageService.Base64ToImage(_currentDatabase.SchemaImage);
                    ImagePlaceholder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ImagePreview.Source = null;
                    ImagePlaceholder.Visibility = Visibility.Visible;
                }

                ApplyInterfaceReadOnlyMode();

                HideDatabaseSelector();
            }
            catch (Exception ex)
            {
                LoadingOverlayContainer.Visibility = Visibility.Collapsed;
                MessageBoxHelper.ShowError($"Ошибка загрузки БД: {ex.Message}", "Ошибка при загрузке");
            }
        }

        private void ToggleEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditAllowed)
            {
                var confirmBox = new CustomMessageBox(
                    "Вы уверены, что хотите разрешить редактирование полей этой базы данных?\n\nИзменение имени или её схемы может повлиять на сохраненную структуру запросов.",
                    "Запрос на редактирование",
                    CustomMessageBoxType.YesNo
                );

                // ИСПРАВЛЕНО: Используем кастомное свойство Result для проверки нажатия кнопки "Да" (Yes)
                confirmBox.ShowDialog();

                if (confirmBox.Result == MessageBoxResult.Yes)
                {
                    _isEditAllowed = true;
                    
                    DatabaseNameTextBox.IsReadOnly = false;
                    DatabaseNameTextBox.Background = new SolidColorBrush(Colors.White);
                    BrowseImageButton.IsEnabled = true; 
                    
                    ToggleEditButton.Content = "🔓 Изменение разрешено";
                    DatabaseNameTextBox.Focus();
                }
            }
            else
            {
                _isEditAllowed = false;
                
                DatabaseNameTextBox.IsReadOnly = true;
                DatabaseNameTextBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));
                BrowseImageButton.IsEnabled = false; 
                
                ToggleEditButton.Content = "🔒 Разрешить изменение";
            }
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

                    double sizeKB = ImageService.GetFileSizeInKB(_selectedImagePath);
                    MessageBoxHelper.ShowInfo($"Изображение загружено\nРазмер: {sizeKB:F1} KB");
                }
                catch
                {
                    MessageBoxHelper.ShowError("Не удалось загрузить изображение. Проверьте формат файла.");
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent?.ShowDatabaseModeView());
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DatabaseNameTextBox.Text))
            {
                MessageBoxHelper.ShowWarning("Введите имя базы данных");
                return;
            }

            try
            {
                var loading = new LoadingOverlay("Сохранение базы данных...");
                LoadingOverlayContainer.Children.Clear();
                LoadingOverlayContainer.Children.Add(loading);
                LoadingOverlayContainer.Visibility = Visibility.Visible;

                if (_selectedOption == "existing" || (_currentDatabase != null && _currentDatabase.IdDatabase > 0))
                {
                    _currentDatabase = await _databaseService.UpdateAsync(
                        _currentDatabase.IdDatabase,
                        DatabaseNameTextBox.Text.Trim(),
                        _selectedImagePath
                    );
                }
                else
                {
                    _currentDatabase = await _databaseService.CreateAsync(
                        DatabaseNameTextBox.Text.Trim(),
                        _selectedImagePath
                    );
                    
                    _selectedOption = "existing"; 
                }

                LoadingOverlayContainer.Visibility = Visibility.Collapsed;

                _parent.CurrentOption.DatabaseId = _currentDatabase.IdDatabase;
                _parent.CurrentOption.DatabaseName = _currentDatabase.Name;
                _parent.CurrentOption.DatabaseSchemaImage = _currentDatabase.SchemaImage;

                FadeOutAndSwitch(() => _parent?.ShowQueryModeView(_parent.CurrentOption.DatabaseId));
            }
            catch (Exception ex)
            {
                LoadingOverlayContainer.Visibility = Visibility.Collapsed;
                MessageBoxHelper.ShowError($"Ошибка сохранения: {ex.Message}");
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
        
        private void RestoreFromState()
        {
            var state = _parent.CurrentOption;

            if (state != null && state.DatabaseId > 0)
            {
                DatabaseNameTextBox.Text = state.DatabaseName;
                SubtitleText.Text = "Настройте выбранную базу данных";

                if (!string.IsNullOrEmpty(state.DatabaseSchemaImage))
                {
                    ImagePreview.Source = ImageService.Base64ToImage(state.DatabaseSchemaImage);
                    ImagePlaceholder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ImagePreview.Source = null;
                    ImagePlaceholder.Visibility = Visibility.Visible;
                }

                _currentDatabase = new Database
                {
                    IdDatabase = state.DatabaseId,
                    Name = state.DatabaseName,
                    SchemaImage = state.DatabaseSchemaImage
                };
            }
        }
    }
}