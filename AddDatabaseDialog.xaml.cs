using System.IO;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PosnaiSQLauncher
{
    public partial class AddDatabaseWindow : Window
    {
        public string DatabaseName => NameBox.Text.Trim();
        public string ImagePath => PathBox.Text.Trim();

        private string _targetFile;
        
        private string _sourceImage;


        public AddDatabaseWindow(Window owner)
        {
            InitializeComponent();
            Owner = owner;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                MessageBox.Show("Сначала введите название базы.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var ofd = new OpenFileDialog
            {
                Title = "Выберите файл диаграммы БД",
                Filter = "Диаграммы|*.png"
            };

            if (ofd.ShowDialog() == true)
            {
                var relative = PathManager.GetRootPath(PathMode.Database);
                var targetFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative));
                Directory.CreateDirectory(targetFolder);
                
                _targetFile = Path.Combine(targetFolder, $"{DatabaseName}_schema.png");
                
                PathBox.Text = ofd.FileName;
                
                PreviewImage.Source = new BitmapImage(new Uri(ofd.FileName));
                
                _sourceImage = ofd.FileName;
            }
        }



        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                MessageBox.Show("Введите название базы.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_sourceImage) || !File.Exists(_sourceImage))
            {
                MessageBox.Show("Выберите файл диаграммы.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            File.Copy(_sourceImage, _targetFile, overwrite: true);

            DialogResult = true;
        }

        
        private void PreviewImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (PreviewImage.Source == null)
                return;

            var win = new Window
            {
                Title = "Просмотр изображения",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Content = new Image
                {
                    Source = PreviewImage.Source,
                    Stretch = Stretch.Uniform
                }
            };

            win.ShowDialog();
        }


    }
}