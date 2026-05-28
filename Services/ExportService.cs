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

        public async Task ExportToJsonAsync(List<ShowOption> data, string filePath)
        {
            string googleSheetUrl = "";
            if (File.Exists("settings.json"))
            {
                string settingsJson = await File.ReadAllTextAsync("settings.json");
                googleSheetUrl = Newtonsoft.Json.Linq.JObject.Parse(settingsJson)["googleSheetUrl"]?.ToString() ?? "";
            }

            var finalJson = new Newtonsoft.Json.Linq.JObject
            {
                ["GoogleSheetUrl"] = googleSheetUrl,
                ["Options"] = Newtonsoft.Json.Linq.JArray.FromObject(data)
            };

            await File.WriteAllTextAsync(filePath, finalJson.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        public async Task ExportToExcelAsync(List<ShowOption> data, string filePath)
        {
            await _excelService.ExportToExcelAsync(data, filePath);
        }
    }
}