// Services/ExcelExportService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClosedXML.Excel;
using PosnaiSQLauncher.Entities;

namespace PosnaiSQLauncher.Services
{
    public class ExcelExportService
    {
        public async Task ExportToExcelAsync(List<ShowOption> data, string filePath)
        {
            await Task.Run(() =>
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Варианты");
                    
                    // Заголовки
                    var headers = new[] 
                    { 
                        "Номер\nварианта", 
                        "Локация", 
                        "Заголовок", 
                        "Условие", 
                        "SQL код", 
                        "Лимит\nв сек." 
                    };
                    
                    for (int col = 1; col <= headers.Length; col++)
                    {
                        var headerCell = worksheet.Cell(1, col);
                        headerCell.Value = headers[col - 1];
                        
                        // Стиль ТОЛЬКО для ячейки с заголовком
                        headerCell.Style.Font.Bold = true;
                        headerCell.Style.Font.FontSize = 12;
                        headerCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0x2196F3);
                        headerCell.Style.Font.FontColor = XLColor.White;
                        headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        headerCell.Style.Alignment.WrapText = true;
                        headerCell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        headerCell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        headerCell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        headerCell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        headerCell.Style.Border.TopBorderColor = XLColor.FromArgb(0x1565C0);
                        headerCell.Style.Border.BottomBorderColor = XLColor.FromArgb(0x1565C0);
                        headerCell.Style.Border.LeftBorderColor = XLColor.FromArgb(0x1565C0);
                        headerCell.Style.Border.RightBorderColor = XLColor.FromArgb(0x1565C0);
                    }
                    
                    // Высота заголовка
                    worksheet.Row(1).Height = 35;
                    
                    // Ширина колонок
                    worksheet.Column(1).Width = 13;  // Номер варианта
                    worksheet.Column(2).Width = 18;  // Локация
                    worksheet.Column(3).Width = 22;  // Заголовок
                    worksheet.Column(4).Width = 28;  // Условие
                    worksheet.Column(5).Width = 38;  // SQL код
                    worksheet.Column(6).Width = 12;  // Лимит в сек.
                    
                    // Данные
                    int row = 2;
                    bool isWhiteRow = true;
                    
                    foreach (var item in data)
                    {
                        worksheet.Cell(row, 1).Value = item.IdOption;
                        worksheet.Cell(row, 2).Value = item.NameLocation;
                        worksheet.Cell(row, 3).Value = item.NameQuery ?? "";
                        worksheet.Cell(row, 4).Value = item.Condition ?? "";
                        worksheet.Cell(row, 5).Value = item.QueryString ?? "";
                        worksheet.Cell(row, 6).Value = item.TimeLimit;
                        
                        // Стиль для строки данных
                        var dataRow = worksheet.Row(row);
                        dataRow.Height = 25;
                        
                        // Шрифт для всех ячеек строки
                        for (int col = 1; col <= 6; col++)
                        {
                            var cell = worksheet.Cell(row, col);
                            cell.Style.Font.FontSize = 11;
                            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                            cell.Style.Alignment.WrapText = true;
                            
                            // Courier New для SQL кода (колонка 5)
                            if (col == 5)
                            {
                                cell.Style.Font.FontName = "Courier New";
                                cell.Style.Font.FontSize = 10;
                            }
                            
                            // Чередующиеся цвета - ТОЛЬКО для ячеек, не для всей строки
                            if (!isWhiteRow)
                            {
                                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xF5F5F5);
                            }
                            else
                            {
                                cell.Style.Fill.BackgroundColor = XLColor.White;
                            }
                            
                            // Границы для всех ячеек
                            cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.TopBorderColor = XLColor.FromArgb(0xE0E0E0);
                            cell.Style.Border.BottomBorderColor = XLColor.FromArgb(0xE0E0E0);
                            cell.Style.Border.LeftBorderColor = XLColor.FromArgb(0xE0E0E0);
                            cell.Style.Border.RightBorderColor = XLColor.FromArgb(0xE0E0E0);
                        }
                        
                        isWhiteRow = !isWhiteRow; // Переключаем цвет для следующей строки
                        row++;
                    }
                    
                    workbook.SaveAs(filePath);
                }
            });
        }
    }
}