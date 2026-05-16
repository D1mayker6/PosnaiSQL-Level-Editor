// Services/DataLoadService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Entities;

namespace PosnaiSQLauncher.Services
{
    public class DataLoadService
    {
        private readonly AppDbContext _context;
        
        // Статические кэши для всех представлений
        public static List<ShowOption> CachedShowOptions { get; private set; } = new();

        public DataLoadService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LoadAllDataAsync()
        {
            // Загружаем все нужные данные из БД один раз
            await LoadShowOptionsAsync();
            // Можешь добавить другие загрузки если нужны
        }

        private async Task LoadShowOptionsAsync()
        {
            try
            {
                CachedShowOptions = await _context.ShowOptions.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при загрузке вариантов: {ex.Message}");
            }
        }
    }
}