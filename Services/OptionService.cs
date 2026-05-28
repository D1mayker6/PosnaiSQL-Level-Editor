using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Entities;

namespace PosnaiSQLauncher.Services
{
    public class OptionService
    {
        private readonly AppDbContext _context;

        public OptionService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Создать новый вариант (Option)
        /// </summary>
        public async Task<Option> CreateAsync(int queryId, int locationId, int timeLimit)
        {
            // Проверяем существование запроса
            bool queryExists = await _context.Queries.AnyAsync(q => q.IdQuery == queryId);
            if (!queryExists)
                throw new Exception($"Запрос с ID {queryId} не найден");

            // Проверяем существование локации
            bool locationExists = await _context.Locations.AnyAsync(l => l.IdLocation == locationId);
            if (!locationExists)
                throw new Exception($"Локация с ID {locationId} не найдена");

            var option = new Option
            {
                IdQuery = queryId,
                IdLocation = locationId,
                TimeLimit = timeLimit
            };

            _context.Options.Add(option);
            await _context.SaveChangesAsync();

            return option;
        }

        /// <summary>
        /// Получить все варианты для запроса
        /// </summary>
        public async Task<List<Option>> GetByQueryIdAsync(int queryId)
        {
            return await _context.Options
                .Where(o => o.IdQuery == queryId)
                .Include(o => o.IdLocationNavigation)
                .OrderBy(o => o.IdOption)
                .ToListAsync();
        }

        /// <summary>
        /// Получить вариант по ID
        /// </summary>
        public async Task<Option> GetByIdAsync(int id)
        {
            var option = await _context.Options
                .Include(o => o.IdLocationNavigation)
                .Include(o => o.IdQueryNavigation)
                .FirstOrDefaultAsync(o => o.IdOption == id);

            if (option == null)
                throw new Exception($"Вариант с ID {id} не найден");

            return option;
        }

        /// <summary>
        /// Обновить вариант
        /// </summary>
        public async Task<Option> UpdateAsync(int id, int? locationId, int timeLimit)
        {
            var option = await _context.Options.FindAsync(id);
            if (option == null)
                throw new Exception($"Вариант с ID {id} не найден");

            bool locationExists = await _context.Locations.AnyAsync(l => l.IdLocation == locationId);
            if (!locationExists)
                throw new Exception($"Локация с ID {locationId} не найдена");

            option.IdLocation = locationId;
            option.TimeLimit = timeLimit;

            await _context.SaveChangesAsync();
            return option;
        }

        /// <summary>
        /// Удалить вариант
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var option = await _context.Options.FindAsync(id);
            if (option == null)
                throw new Exception($"Вариант с ID {id} не найден");

            _context.Options.Remove(option);
            await _context.SaveChangesAsync();
        }
    }
}