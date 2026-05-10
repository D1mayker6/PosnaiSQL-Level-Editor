using System.Windows;
using System.Windows.Controls;

namespace PosnaiSQLauncher
{
    public partial class NewMainWindow : Window
    {
        
        public NewMainWindow()
        {
            InitializeComponent();
            ShowLoginScreen();

        }
        
        private void ShowLoginScreen()
        {
            var loginView = new LoginView();
            loginView.LoginSuccessful += (s, e) =>
            { 
                ShowMainMenu();
            };
            MainContent.Content = loginView;
        
            SettingsButton.Visibility = Visibility.Collapsed;
        }

        public void ShowMainMenu()
        {
            MainContent.Content = new NewMainMenuView(this);
            SettingsButton.Visibility = Visibility.Visible;
        }

        public void ShowCreateOption()
        {
            MainContent.Content = new NewCreateOptionView(this);
        }

        public void ShowEditOption()
        {
            MainContent.Content = new NewEditOptionView(this);
        }
        
        public void ShowQueryView()
        {
            MainContent.Content = new NewQueryView(this);
        }
        
        public void ShowLevelView() 
        { 
            MainContent.Content = new NewLevelView(this); 
        }
        
        public void ShowExportView() 
        { 
            MainContent.Content = new NewExportView(this); 
        }
        
        public void ShowSaveView() 
        { 
            MainContent.Content = new NewSaveView(this); 
        }
        
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SettingsView(this);
            MessageBox.Show("Открытие настроек");
        }

        public void ShowDatabaseConfig(string option)
        {
            MainContent.Content = new DatabaseConfigView(this, option);
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