// Services/ExportService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Entities;

namespace PosnaiSQLauncher.Services
{
    public class ExportService
    {
        private readonly AppDbContext _context;
        private readonly ExcelExportService _excelService;

        public ExportService(AppDbContext context)
        {
            _context = context;
            _excelService = new ExcelExportService();
        }

        // Получить все данные из ShowOption
        public async Task<List<ShowOption>> GetAllAsync()
        {
            return await _context.ShowOptions.ToListAsync();
        }

        // Получить данные по ID
        public async Task<List<ShowOption>> GetExportDataAsync(List<int> selectedIds)
        {
            return await _context.ShowOptions
                .Where(o => selectedIds.Contains(o.IdOption))
                .ToListAsync();
        }

        // Экспорт в JSON
        public async Task ExportToJsonAsync(List<ShowOption> data, string filePath)
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true, 
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
            };
            string json = JsonSerializer.Serialize(data, options);
            await File.WriteAllTextAsync(filePath, json);
        }

        // Экспорт в Excel
        public async Task ExportToExcelAsync(List<ShowOption> data, string filePath)
        {
            await _excelService.ExportToExcelAsync(data, filePath);
        }
    }
}