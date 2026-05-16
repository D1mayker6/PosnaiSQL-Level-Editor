// SettingsView.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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

        private void SaveSettings()
        {
            try
            {
                var settings = new JObject
                {
                    ["googleSheetUrl"] = GoogleSheetUrlTextBox.Text.Trim()
                };

                File.WriteAllText(SETTINGS_FILE, settings.ToString());
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка сохранения настроек: {ex.Message}");
            }
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            MessageBoxHelper.ShowSuccess("Настройки успешно сохранены!");
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Показываем диалог смены пароля
            var dialog = new ChangePasswordDialog();
            if (dialog.ShowDialog() == true)
            {
                string oldPassword = dialog.OldPassword;
                string newPassword = dialog.NewPassword;

                if (VerifyAndChangePassword(oldPassword, newPassword))
                {
                    MessageBoxHelper.ShowSuccess("Пароль успешно изменён!");
                    
                    // Возвращаем пользователя в окно авторизации
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