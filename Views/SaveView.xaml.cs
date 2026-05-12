using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace PosnaiSQLauncher
{
    public partial class SaveView : UserControl
    {
        private MainWindow _parent;

        public SaveView(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent?.ShowLevelView()); // Возврат на этап 3
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Здесь будет твоя логика записи в БД или файл
            MessageBox.Show("Вариант успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            
            FadeOutAndSwitch(() => _parent?.ShowMainMenu()); // В главное меню
        }

        private void FadeOutAndSwitch(Action switchAction)
        {
            var fadeOut = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(300) };
            var storyboard = new Storyboard();
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOut);
            storyboard.Completed += (s, ev) => switchAction();
            storyboard.Begin(this);
        }
    }
}