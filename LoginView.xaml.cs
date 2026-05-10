using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace PosnaiSQLauncher
{
    public partial class LoginView : UserControl
    {
        private const string CONFIG_FILE = "auth.json";

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
                // Убираем очистку полей - просто показываем ошибку
            }
        }

        private bool ValidateCredentials(string login, string password)
        {
            try
            {
                if (!File.Exists(CONFIG_FILE))
                {
                    // Создаем файл с дефолтными данными: admin/admin
                    var defaultAuth = new JObject
                    {
                        ["login"] = "admin",
                        ["password"] = "admin"
                    };
                    File.WriteAllText(CONFIG_FILE, defaultAuth.ToString());
                }

                string json = File.ReadAllText(CONFIG_FILE);
                JObject authData = JObject.Parse(json);

                string storedLogin = authData["login"]?.ToString();
                string storedPassword = authData["password"]?.ToString();

                return login == storedLogin && password == storedPassword;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка чтения файла авторизации: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
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
            // Скрываем ошибку при вводе нового текста
            if (ErrorMessage.Visibility == Visibility.Visible)
                ErrorMessage.Visibility = Visibility.Collapsed;
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Скрываем ошибку при вводе нового текста
            if (ErrorMessage.Visibility == Visibility.Visible)
                ErrorMessage.Visibility = Visibility.Collapsed;
        }
    }
}