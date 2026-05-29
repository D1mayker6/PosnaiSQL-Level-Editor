using System.Windows;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Models;
using PosnaiSQLauncher.Services;
using PosnaiSQLauncher.Helpers;

namespace PosnaiSQLauncher
{
    public partial class MainWindow : Window
    {
        public OptionData CurrentOption { get; set; } = new OptionData();
        private AppDbContext _dbContext;
        private DataLoadService _dataLoadService;
        
        public MainWindow()
        {
            InitializeComponent();
            ShowLoginScreen();
        }
        
        public void ShowLoginScreen()
        {
            var loginView = new LoginView();
            loginView.LoginSuccessful += LoginView_LoginSuccessful;
            MainContent.Content = loginView;
            SettingsButton.Visibility = Visibility.Collapsed;
        }

        private void LoginView_LoginSuccessful(object sender, EventArgs e)
        {
            InitializeDatabaseAndLoadData();
        }

        private async void InitializeDatabaseAndLoadData()
        {
            try
            {
                ShowLoadingScreen("Загрузка данных...");

                _dbContext = new AppDbContext();
                _dataLoadService = new DataLoadService(_dbContext);

                await _dataLoadService.LoadAllDataAsync();

                HideLoadingScreen();
                ShowMainMenu();
            }
            catch (Exception ex)
            {
                HideLoadingScreen();
                MessageBoxHelper.ShowError($"Ошибка загрузки данных: {ex.Message}");
                ShowLoginScreen();
            }
        }

        private void ShowLoadingScreen(string message)
        {
            var loadingOverlay = new LoadingOverlay(message);
            MainContent.Content = loadingOverlay;
            SettingsButton.Visibility = Visibility.Collapsed;
        }

        private void HideLoadingScreen()
        {
        }

        public void ShowMainMenu()
        {
            MainContent.Content = new MainMenuView(this);
            SettingsButton.Visibility = Visibility.Visible;
        }

        public void ShowDatabaseModeView()
        {
            MainContent.Content = new DatabaseModeView(this);
            SettingsButton.Visibility = Visibility.Collapsed;
        }

        public void ShowEditOption()
        {
            MainContent.Content = new EditOptionView(this);
            SettingsButton.Visibility = Visibility.Collapsed;
        }
        
        public void ShowDatabaseConfig(string option)
        {
            MainContent.Content = new DatabaseView(this, option);
        }

        public void ShowQueryModeView(int databaseId)
        {
            MainContent.Content = new QueryModeView(this, databaseId);
            SettingsButton.Visibility = Visibility.Collapsed;
        }

        public void ShowQueryView(int databaseId, string mode = "new")
        {
            MainContent.Content = new QueryView(this, databaseId, mode);
        }

        public void ShowQueryView(int databaseId)
        {
            ShowQueryView(databaseId, "new");
        }

        public void ShowQueryView()
        {
            ShowQueryView(0);
        }
        
        public void ShowLevelView(int queryId)
        {
            MainContent.Content = new LevelView(this, queryId);
            SettingsButton.Visibility = Visibility.Collapsed;
        }

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
            SettingsButton.Visibility = Visibility.Collapsed;
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