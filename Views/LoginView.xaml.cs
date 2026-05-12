using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using BCrypt.Net;

namespace PosnaiSQLauncher
{
    public partial class LoginView : UserControl
    {
        private const string CONFIG_FILE = "databin.json";

        public event EventHandler LoginSuccessful;

        public LoginView()
        {
            InitializeComponent();
            LoginTextBox.Focus();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = ShowPasswordToggle.IsChecked == true
                ? PasswordTextBox.Text
                : PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Заполните все поля");
                return;
            }

            if (ValidateCredentials(login, password))
            {
                ErrorMessage.Visibility = Visibility.Collapsed;
                LoginSuccessful?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ShowError("Неверный логин или пароль");
            }
        }

        private bool ValidateCredentials(string login, string password)
        {
            try
            {
                if (!File.Exists(CONFIG_FILE))
                {
                    CreateDefaultConfig();
                }

                string json = File.ReadAllText(CONFIG_FILE);
                JObject authData = JObject.Parse(json);

                string storedLogin = authData["DataStream"]?.ToString();
                string storedPassword = authData["InfoVault"]?.ToString();

                if (storedLogin == null || storedPassword == null)
                    return false;

                // Сравниваем логин и проверяем хэш пароля
                bool loginMatch = login == storedLogin;
                bool passwordMatch = BCrypt.Net.BCrypt.Verify(password, storedPassword);

                return loginMatch && passwordMatch;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка чтения файла авторизации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void CreateDefaultConfig()
        {
            // Хэшируем дефолтный пароль "fik" перед сохранением
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("fik");

            var defaultAuth = new JObject
            {
                ["DataStream"] = "fik",       // логин
                ["InfoVault"] = hashedPassword // пароль в виде хэша
            };

            File.WriteAllText(CONFIG_FILE, defaultAuth.ToString());
        }

        // Метод для смены пароля (пригодится)
        public void ChangePassword(string newPassword)
        {
            string json = File.ReadAllText(CONFIG_FILE);
            JObject authData = JObject.Parse(json);

            authData["InfoVault"] = BCrypt.Net.BCrypt.HashPassword(newPassword);
            File.WriteAllText(CONFIG_FILE, authData.ToString());
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }

        private void ShowPasswordToggle_Checked(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordTextBox.Visibility = Visibility.Visible;
            PasswordTextBox.Focus();
            PasswordTextBox.CaretIndex = PasswordTextBox.Text.Length;
        }

        private void ShowPasswordToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordTextBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordBox.Focus();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ErrorMessage.Visibility == Visibility.Visible)
                ErrorMessage.Visibility = Visibility.Collapsed;
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ErrorMessage.Visibility == Visibility.Visible)
                ErrorMessage.Visibility = Visibility.Collapsed;
        }
    }
}