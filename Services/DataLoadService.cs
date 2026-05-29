using Microsoft.EntityFrameworkCore;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Entities;

namespace PosnaiSQLauncher.Services
{
    public class DataLoadService
    {
        private readonly AppDbContext _context;
        
        public static List<ShowOption> CachedShowOptions { get; private set; } = new();

        public DataLoadService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LoadAllDataAsync()
        {
            await LoadShowOptionsAsync();
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