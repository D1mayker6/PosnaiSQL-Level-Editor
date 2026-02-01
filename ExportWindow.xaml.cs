using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Newtonsoft.Json;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Entities;

namespace PosnaiSQLauncher;

public partial class ExportWindow : Window
{
    private List<Option> _allOptions;
    private List<Option> _filteredOptions;

    public ExportWindow(List<Option> options)
    {
        InitializeComponent();
        _allOptions = options;
        _filteredOptions = options;
        RenderOptions();
    }

    private void RenderOptions()
    {
        OptionsPanel.Children.Clear();

        foreach (var option in _filteredOptions)
        {
            var cb = new CheckBox
            {
                Content = $"Вариант {option.IdOption}",
                Tag = option,
                Margin = new Thickness(4),
                FontSize = 16
            };
            OptionsPanel.Children.Add(cb);
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string query = SearchBox.Text.Trim();
        if (string.IsNullOrEmpty(query))
        {
            _filteredOptions = _allOptions;
        }
        else
        {
            _filteredOptions = _allOptions
                .Where(o => o.IdOption.ToString().Contains(query))
                .ToList();
        }

        RenderOptions();
    }

    private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        foreach (var child in OptionsPanel.Children)
            if (child is CheckBox cb)
                cb.IsChecked = true;
    }

    private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        foreach (var child in OptionsPanel.Children)
            if (child is CheckBox cb)
                cb.IsChecked = false;
    }
    
    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        string format = (ExportFormatCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
        var path = PathManager.GetRootPath(PathMode.Option);
        switch (format)
        {
            case "EXCEL":
                ExportToExcel(path);
                break;

            case "JSON":
                ExportToJson(path);
                break;
        }
    }

    private void ExportToExcel(string path)
    {
                var selectedIds = OptionsPanel.Children
                .OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .Select(cb => ((Option)cb.Tag).IdOption)
                .ToList();

            if (!selectedIds.Any())
            {
                MessageBox.Show(
                    "Не выбраны варианты для экспорта.",
                    "Экспорт",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            using var db = new AppDbContext();
            var data = db.Set<ShowOption>()
                         .Where(o => selectedIds.Contains(o.IdOption))
                         .ToList();

            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Export");

            string[] headers = {
                "Номер варианта", "Локация", "База данных", "Название запроса",
                "Условие запроса", "Запрос", "Сложность", "Лимит времени"
            };
            
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 13;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                cell.Style.Fill.BackgroundColor = XLColor.Green;
            }
            
            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                ws.Row(row).Height = 100;

                ws.Cell(row, 1).Value = data[i].IdOption;
                ws.Cell(row, 2).Value = data[i].NameLocation;
                ws.Cell(row, 3).Value = data[i].NameDatabase;
                ws.Cell(row, 4).Value = data[i].DecryptedName;
                ws.Cell(row, 5).Value = data[i].DecryptedCondition;
                ws.Cell(row, 6).Value = data[i].DecryptedQueryString;
                ws.Cell(row, 7).Value = data[i].Difficulty;
                ws.Cell(row, 8).Value = data[i].TimeLimit.ToString();

                for (int col = 1; col <= 8; col++)
                {
                    var cell = ws.Cell(row, col);
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    cell.Style.Font.FontSize = 12;
                }
                
                ws.Cell(row, 1).Style.Font.FontSize = 24;
                ws.Cell(row, 1).Style.Font.Bold = true;

                ws.Cell(row, 7).Style.Font.FontSize = 24;
                ws.Cell(row, 7).Style.Font.Bold = true;

                ws.Cell(row, 6).Style.Font.FontName = "Courier New";
                ws.Cell(row, 6).Style.Alignment.WrapText = true;
            }

            ws.Column(3).Width = 30;
            ws.Column(4).Width = 35;
            ws.Column(5).Width = 45;
            ws.Column(8).Width = 20;

            ws.Columns(1, 2).AdjustToContents();
            ws.Columns(6, 7).AdjustToContents();

            string tempPath = Path.Combine(path, "EXCEL");
            Directory.CreateDirectory(tempPath);
            string fileName = $"Options_{DateTime.Now:yyyy-MM-dd, HH-mm}.xlsx";
            string fullPath = Path.Combine(tempPath, fileName);
            wb.SaveAs(fullPath);
            Process.Start(new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            });
    }

    private void ExportToJson(string path)
    {
            var selectedIds = OptionsPanel.Children
        .OfType<CheckBox>()
        .Where(cb => cb.IsChecked == true)
        .Select(cb => ((Option)cb.Tag).IdOption)
        .ToList();

    if (!selectedIds.Any())
    {
        MessageBox.Show(
            "Не выбраны варианты для экспорта.",
            "Экспорт",
            MessageBoxButton.OK,
            MessageBoxImage.Warning
        );
        return;
    }

    using var db = new AppDbContext();
    var data = db.Set<ShowOption>()
        .Where(o => selectedIds.Contains(o.IdOption))
        .ToList();

    var exportData = data.Select(o => new
    {
        IdOption = o.IdOption,
        NameLocation = o.NameLocation,
        NameDatabase = o.NameDatabase,
        DecryptedName = o.DecryptedName,
        DecryptedCondition = o.DecryptedCondition,
        DecryptedQueryString = o.DecryptedQueryString,
        Difficulty = o.Difficulty,
        TimeLimit = o.TimeLimit
    }).ToList();

    string json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
    string tempPath = Path.Combine(path, "JSON");
    Directory.CreateDirectory(tempPath);
    string fileName = $"Options_{DateTime.Now:yyyy-MM-dd, HH-mm}.json";
    string fullPath = Path.Combine(tempPath, fileName);
    File.WriteAllText(fullPath, json);

    Process.Start(new ProcessStartInfo
    {
        FileName = fullPath,
        UseShellExecute = true
    });
    }
}
