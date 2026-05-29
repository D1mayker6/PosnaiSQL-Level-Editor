using System.Diagnostics; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Entities;
using PosnaiSQLauncher.Services;
using PosnaiSQLauncher.Helpers;

namespace PosnaiSQLauncher
{
    public partial class ExportView : UserControl
    {
        private MainWindow _parent;
        private string _selectedFormat = "";
        private readonly ExportService _exportService;
        private List<ShowOption> _allVariants = new List<ShowOption>();
        private bool _isUpdatingSelectAll = false;

        public ExportView(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            _exportService = new ExportService(new AppDbContext());
            
            ShowWindow();
            LoadVariantsAsync();
        }

        private void ShowWindow()
        {
            var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
            this.BeginAnimation(OpacityProperty, anim);
        }

        private async void LoadVariantsAsync()
        {
            try
            {
                ShowLoading("Загрузка вариантов...");
                _allVariants = await _exportService.GetAllAsync();
                UpdateList(_allVariants);
                HideLoading();
            }
            catch (Exception ex)
            {
                HideLoading();
                MessageBoxHelper.ShowError("Ошибка загрузки: " + ex.Message);
            }
        }

        private void UpdateList(List<ShowOption> items)
        {
            VariantsListBox.Items.Clear();
            foreach (var item in items)
            {
                var displayText = $"Вариант № {item.IdOption}: {item.NameQuery ?? "Без названия"} ({item.NameLocation})";
                VariantsListBox.Items.Add(new ListBoxItem 
                { 
                    Content = displayText,
                    Tag = item.IdOption
                });
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchBox.Text.ToLower();
            var filtered = _allVariants.Where(v => 
                v.IdOption.ToString().Contains(query) || 
                (v.NameQuery?.ToLower().Contains(query) ?? false) || 
                (v.NameLocation?.ToLower().Contains(query) ?? false) ||
                (v.Condition?.ToLower().Contains(query) ?? false)
            ).ToList();
            
            UpdateList(filtered);
            UpdateSelectAllCheckbox();
        }

        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingSelectAll) return;

            _isUpdatingSelectAll = true;
            foreach (ListBoxItem item in VariantsListBox.Items)
            {
                item.IsSelected = true;
            }
            _isUpdatingSelectAll = false;

            VariantsListBox_SelectionChanged(null, null);
        }

        private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingSelectAll) return;

            _isUpdatingSelectAll = true;
            foreach (ListBoxItem item in VariantsListBox.Items)
            {
                item.IsSelected = false;
            }
            _isUpdatingSelectAll = false;

            VariantsListBox_SelectionChanged(null, null);
        }

        private void UpdateSelectAllCheckbox()
        {
            if (_isUpdatingSelectAll) return;

            _isUpdatingSelectAll = true;

            int totalItems = VariantsListBox.Items.Count;
            int selectedItems = VariantsListBox.SelectedItems.Count;

            if (totalItems == 0)
            {
                SelectAllCheckBox.IsChecked = false;
            }
            else if (selectedItems == totalItems)
            {
                SelectAllCheckBox.IsChecked = true;
            }
            else
            {
                SelectAllCheckBox.IsChecked = false;
            }

            _isUpdatingSelectAll = false;
        }

        private void CardJson_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _selectedFormat = "json";
            PathTextBox.Text = "";
            HighlightCard(CardJson, CardExcel, "#E65100");
        }

        private void CardExcel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _selectedFormat = "excel";
            PathTextBox.Text = "";
            HighlightCard(CardExcel, CardJson, "#2E7D32");
        }

        private void HighlightCard(Border selected, Border other, string colorHex)
        {
            var activeColor = (Color)ColorConverter.ConvertFromString(colorHex);
            var inactiveColor = (Color)ColorConverter.ConvertFromString("#E0E0E0");
            AnimateBorder(selected, activeColor, 3);
            AnimateBorder(other, inactiveColor, 2);
            ValidateExport();
        }

        private void AnimateBorder(Border border, Color targetColor, double targetThickness)
        {
            ColorAnimation colorAnim = new ColorAnimation { To = targetColor, Duration = TimeSpan.FromMilliseconds(300) };
            ThicknessAnimation thickAnim = new ThicknessAnimation { To = new Thickness(targetThickness), Duration = TimeSpan.FromMilliseconds(300) };
            if (border.BorderBrush is SolidColorBrush currentBrush)
            {
                if (currentBrush.IsFrozen) border.BorderBrush = currentBrush.Clone();
                border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }
            border.BeginAnimation(Border.BorderThicknessProperty, thickAnim);
        }

        private void VariantsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
            SelectedCountText.Text = $"Выбрано: {VariantsListBox.SelectedItems.Count}";
            UpdateSelectAllCheckbox();
            ValidateExport();
        }

        private void ValidateExport() 
        {
            ExportButton.IsEnabled = VariantsListBox.SelectedItems.Count > 0 
                                  && !string.IsNullOrEmpty(_selectedFormat) 
                                  && !string.IsNullOrEmpty(PathTextBox.Text);
        }

        private void Browse_Click(object sender, RoutedEventArgs e) 
        {
            if (string.IsNullOrEmpty(_selectedFormat))
            {
                MessageBoxHelper.ShowWarning("Сначала выберите формат (JSON или Excel)");
                return;
            }

            SaveFileDialog dlg = new SaveFileDialog 
            { 
                Filter = _selectedFormat == "excel" 
                    ? "Excel Workbook|*.xlsx" 
                    : "JSON Files|*.json",
                FileName = $"Export_Option_{DateTime.Now:dd.MM.yyyy - HH.mm.ss}",
                DefaultExt = _selectedFormat == "excel" ? ".xlsx" : ".json"
            };

            if (dlg.ShowDialog() == true) 
            { 
                PathTextBox.Text = dlg.FileName; 
                ValidateExport(); 
            }
        }
        
        private void SelectAllBorder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectAllCheckBox.IsChecked = !SelectAllCheckBox.IsChecked;
        }

        private async void Export_Click(object sender, RoutedEventArgs e) 
        {
            try 
            {
                ShowLoading("Выполняется экспорт...");
                
                var selectedIds = VariantsListBox.SelectedItems
                    .Cast<ListBoxItem>()
                    .Select(i => (int)i.Tag)
                    .ToList();

                var dataToExport = _allVariants
                    .Where(v => selectedIds.Contains(v.IdOption))
                    .ToList();

                if (_selectedFormat == "json")
                {
                    await _exportService.ExportToJsonAsync(dataToExport, PathTextBox.Text);
                }
                else
                {
                    await _exportService.ExportToExcelAsync(dataToExport, PathTextBox.Text);
                }

                HideLoading();
                MessageBoxHelper.ShowSuccess($"Экспорт завершен успешно!\nФайл сохранен: {PathTextBox.Text}");
                
                OpenFile(PathTextBox.Text);
                
                _parent.ShowMainMenu();
            }
            catch (Exception ex)
            {
                HideLoading();
                MessageBoxHelper.ShowError("Ошибка при экспорте: " + ex.Message);
            }
        }

        private void OpenFile(string filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowWarning($"Не удалось открыть файл: {ex.Message}");
            }
        }

        private void ShowLoading(string text)
        {
            LoadingOverlayContainer.Children.Clear();
            LoadingOverlayContainer.Children.Add(new LoadingOverlay(text));
            LoadingOverlayContainer.Visibility = Visibility.Visible;
        }

        private void HideLoading() => LoadingOverlayContainer.Visibility = Visibility.Collapsed;

        private void Back_Click(object sender, RoutedEventArgs e) 
        {
            var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            anim.Completed += (s, ev) => _parent.ShowMainMenu();
            this.BeginAnimation(OpacityProperty, anim);
        }
    }
}