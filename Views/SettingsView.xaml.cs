// SettingsView.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using PosnaiSQLauncher.Helpers;

namespace PosnaiSQLauncher
{
    public partial class SettingsView : UserControl
    {
        private MainWindow _parent;
        private const string SETTINGS_FILE = "settings.json";
        private const string CONFIG_FILE = "databin.json";

        public SettingsView(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SETTINGS_FILE))
                {
                    string json = File.ReadAllText(SETTINGS_FILE);
                    JObject settings = JObject.Parse(json);
                    GoogleSheetUrlTextBox.Text = settings["googleSheetUrl"]?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка загрузки настроек: {ex.Message}");
            }
        }

        private bool ValidateGoogleSheetUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                ShowValidationError("Ссылка не может быть пустой");
                return false;
            }

            // Проверяем что это Google Sheets URL
            if (!url.Contains("docs.google.com/spreadsheets"))
            {
                ShowValidationError("❌ Ссылка должна быть на Google Table (docs.google.com/spreadsheets)");
                return false;
            }

            // Проверяем что в URL есть ID таблицы
            if (!Regex.IsMatch(url, @"/spreadsheets/d/[a-zA-Z0-9-_]+"))
            {
                ShowValidationError("❌ Некорректный формат ссылки на Google Table");
                return false;
            }

            ShowValidationSuccess("✓ Ссылка прошла валидацию");
            return true;
        }

        private void ShowValidationError(string message)
        {
            ValidationMessage.Text = message;
            ValidationMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 229, 57, 53)); // Красный
            ValidationMessage.Visibility = Visibility.Visible;
        }

        private void ShowValidationSuccess(string message)
        {
            ValidationMessage.Text = message;
            ValidationMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 46, 125, 50)); // Зеленый
            ValidationMessage.Visibility = Visibility.Visible;
        }

        private void CopyUrl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = GoogleSheetUrlTextBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(url))
                {
                    Clipboard.SetText(url);
                    MessageBoxHelper.ShowSuccess("Ссылка скопирована в буфер обмена!");
                }
                else
                {
                    MessageBoxHelper.ShowWarning("Поле ссылки пустое!");
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка копирования: {ex.Message}");
            }
        }

        private void OpenUrl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = GoogleSheetUrlTextBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(url))
                {
                    if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        MessageBoxHelper.ShowWarning("Некорректная ссылка!");
                    }
                }
                else
                {
                    MessageBoxHelper.ShowWarning("Поле ссылки пустое!");
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка открытия ссылки: {ex.Message}");
            }
        }

        private void SaveUrl_Click(object sender, RoutedEventArgs e)
        {
            string url = GoogleSheetUrlTextBox.Text.Trim();

            if (!ValidateGoogleSheetUrl(url))
                return;

            // Показываем кастомный диалог подтверждения
            var dialog = new CustomMessageBox(
                "Подтвердите, что эта ссылка имеет права на редактирование.",
                "Подтверждение",
                CustomMessageBoxType.YesNo);

            if (dialog.ShowDialog() != true || dialog.Result != MessageBoxResult.Yes)
                return;

            try
            {
                var settings = new JObject
                {
                    ["googleSheetUrl"] = url
                };

                File.WriteAllText(SETTINGS_FILE, settings.ToString());
                MessageBoxHelper.ShowSuccess("Ссылка успешно сохранена!");
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ChangePasswordDialog();
            if (dialog.ShowDialog() == true)
            {
                string oldPassword = dialog.OldPassword;
                string newPassword = dialog.NewPassword;

                if (VerifyAndChangePassword(oldPassword, newPassword))
                {
                    MessageBoxHelper.ShowSuccess("Пароль успешно изменён!");
                    _parent.ShowLoginScreen();
                }
                else
                {
                    MessageBoxHelper.ShowError("Неверный старый пароль!");
                }
            }
        }

        private bool VerifyAndChangePassword(string oldPassword, string newPassword)
        {
            try
            {
                if (!File.Exists(CONFIG_FILE))
                    return false;

                string json = File.ReadAllText(CONFIG_FILE);
                JObject authData = JObject.Parse(json);

                string storedPassword = authData["InfoVault"]?.ToString();

                if (storedPassword == null || !BCrypt.Net.BCrypt.Verify(oldPassword, storedPassword))
                    return false;

                authData["InfoVault"] = BCrypt.Net.BCrypt.HashPassword(newPassword);
                
                File.WriteAllText(CONFIG_FILE, authData.ToString());

                return true;
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка изменения пароля: {ex.Message}");
                return false;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _parent.ShowMainMenu();
        }
    }
}