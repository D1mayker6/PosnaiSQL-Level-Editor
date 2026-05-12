using System.Windows;
using System.Windows.Controls;

namespace PosnaiSQLauncher
{
    public partial class MainWindow : Window
    {
        
        public MainWindow()
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
            MainContent.Content = new MainMenuView(this);
            SettingsButton.Visibility = Visibility.Visible;
        }

        public void ShowCreateOption()
        {
            MainContent.Content = new CreateOptionView(this);
            SettingsButton.Visibility = Visibility.Collapsed;
        }

        public void ShowEditOption()
        {
            MainContent.Content = new EditOptionView(this);
            SettingsButton.Visibility = Visibility.Collapsed;
        }
        
        public void ShowDatabaseConfig(string option)
        {
            MainContent.Content = new DatabaseConfigView(this, option);
        }

        public void ShowQueryView(int databaseId)
        {
            MainContent.Content = new QueryView(this, databaseId);
        }

        public void ShowQueryView()
        {
            ShowQueryView(0);
        }
        
        // ← НОВОЕ: Перегрузка с queryId
        public void ShowLevelView(int queryId)
        {
            MainContent.Content = new LevelView(this, queryId);
            SettingsButton.Visibility = Visibility.Collapsed;
        }

        // Старый метод для совместимости
        public void ShowLevelView() 
        { 
            ShowLevelView(0);
        }
        
        public void ShowExportView() 
        { 
            MainContent.Content = new ExportView(this); 
            SettingsButton.Visibility = Visibility.Collapsed;
        }
        
        public void ShowSaveView() 
        { 
            MainContent.Content = new SaveView(this); 
        }
        
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SettingsView(this);
            SettingsButton.Visibility = Visibility.Collapsed;
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