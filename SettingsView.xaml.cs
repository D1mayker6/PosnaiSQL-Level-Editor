using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace PosnaiSQLauncher
{
    public partial class SettingsView : UserControl
    {
        private NewMainWindow _parent;
        private const string SETTINGS_FILE = "settings.json";

        public SettingsView(NewMainWindow parent)
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
                MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("Ссылка скопирована в буфер обмена!", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Поле ссылки пустое!", 
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка копирования: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show("Некорректная ссылка!", 
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Поле ссылки пустое!", 
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия ссылки: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateTable_Click(object sender, RoutedEventArgs e)
        {
            // Здесь будет логика генерации таблицы результатов
            MessageBox.Show("Функция генерации таблицы будет реализована позже.", 
                "Генерация таблицы", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            MessageBox.Show("Настройки успешно сохранены!", 
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _parent.ShowMainMenu();
        }
    }
}