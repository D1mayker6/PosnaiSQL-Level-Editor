using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace PosnaiSQLauncher
{
    public partial class NewMainWindow : Window
    {
        public NewMainWindow()
        {
            InitializeComponent();
            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            MainContent.Content = new NewMainMenuView(this);
        }

        public void ShowCreateOption()
        {
            // Если есть старый контент - анимируем выход
            if (MainContent.Content is UserControl oldContent)
            {
                var fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = new System.TimeSpan(0, 0, 0, 0, 300)
                };

                var storyboard = new Storyboard();
                Storyboard.SetTargetProperty(fadeOut, new System.Windows.PropertyPath("Opacity"));
                storyboard.Children.Add(fadeOut);

                storyboard.Completed += (s, e) =>
                {
                    // После выхода старого контента загружаем новый
                    MainContent.Content = new NewCreateOptionView(this);
                };

                // Анимируем выход старого контента
                storyboard.Begin(oldContent);
            }
            else
            {
                // Если нет старого контента - просто загружаем новый
                MainContent.Content = new NewCreateOptionView(this);
            }
        }

        public void ShowEditOption()
        {
            if (MainContent.Content is UserControl oldContent)
            {
                var fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = new System.TimeSpan(0, 0, 0, 0, 300)
                };

                var storyboard = new Storyboard();
                Storyboard.SetTargetProperty(fadeOut, new System.Windows.PropertyPath("Opacity"));
                storyboard.Children.Add(fadeOut);

                storyboard.Completed += (s, e) =>
                {
                    MainContent.Content = new NewEditOptionView(this);
                };

                storyboard.Begin(oldContent);
            }
            else
            {
                MainContent.Content = new NewEditOptionView(this);
            }
        }

        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                MaximizeBtn.Content = "❒";
            }
            else
            {
                WindowState = WindowState.Normal;
                MaximizeBtn.Content = "☐";
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}