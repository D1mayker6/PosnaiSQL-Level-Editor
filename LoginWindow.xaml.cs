using System.IO;
using System.Text.Json;
using System.Windows;

namespace PosnaiSQLauncher
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "databin.json");
            var json = File.ReadAllText(jsonPath);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string encryptedLogin = PathManager.GetRootPath(PathMode.Login);
            string encryptedPassword = PathManager.GetRootPath(PathMode.Password);

            string storedLogin = CryptoManager.SafeDecrypt(encryptedLogin);
            string storedPassword = CryptoManager.SafeDecrypt(encryptedPassword);

            
            if (login == storedLogin && password == storedPassword)
            {
                var editor = new LevelEditor();
                editor.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}