using System.IO;
using System.Text.Json;
using System.Windows;

namespace PosnaiSQLauncher
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly string _jsonPath = "databin.json";

        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = OldPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                MessageBox.Show("Новый пароль должен быть не короче 6 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _jsonPath);
            var json = File.ReadAllText(jsonPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string encryptedLogin = root.GetProperty("DataStream").GetString();
            string encryptedPassword = root.GetProperty("InfoVault").GetString();

            string storedPassword = CryptoManager.SafeDecrypt(encryptedPassword);

            if (oldPassword != storedPassword)
            {
                MessageBox.Show("Старый пароль неверен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            var dict = new Dictionary<string, string>();
            
            foreach (var prop in root.EnumerateObject())
                dict[prop.Name] = prop.Value.GetString();
            
            string newEncryptedPassword = CryptoManager.Encrypt(newPassword);
            
            dict["InfoVault"] = newEncryptedPassword;
            var updatedJson = JsonSerializer.Serialize(dict);
            File.WriteAllText(_jsonPath, updatedJson);

            MessageBox.Show("Пароль успешно изменён.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
    }
}
