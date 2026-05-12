using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Entities;

namespace PosnaiSQLauncher.Services
{
    public class DatabaseService
    {
        private readonly AppDbContext _context;

        public DatabaseService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Создать новую базу данных
        /// </summary>
        public async Task<Database> CreateAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя базы данных не может быть пустым");

            // Проверка на дубликаты
            bool exists = await _context.Databases.AnyAsync(d => d.Name == name);
            if (exists)
                throw new Exception($"База данных с именем '{name}' уже существует");

            var database = new Database
            {
                Name = name.Trim()
            };

            _context.Databases.Add(database);
            await _context.SaveChangesAsync();

            return database;
        }

        /// <summary>
        /// Получить все базы данных
        /// </summary>
        public async Task<List<Database>> GetAllAsync()
        {
            return await _context.Databases
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Получить базу по ID
        /// </summary>
        public async Task<Database> GetByIdAsync(int id)
        {
            var database = await _context.Databases
                .Include(d => d.Queries)
                .FirstOrDefaultAsync(d => d.IdDatabase == id);

            if (database == null)
                throw new Exception($"База данных с ID {id} не найдена");

            return database;
        }

        /// <summary>
        /// Обновить название базы данных
        /// </summary>
        public async Task<Database> UpdateAsync(int id, string newName)
        {
            var database = await _context.Databases.FindAsync(id);
            if (database == null)
                throw new Exception($"База данных с ID {id} не найдена");

            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Имя базы данных не может быть пустым");

            database.Name = newName.Trim();
            await _context.SaveChangesAsync();

            return database;
        }

        /// <summary>
        /// Удалить базу данных (с проверкой связей)
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var database = await _context.Databases
                .Include(d => d.Queries)
                .FirstOrDefaultAsync(d => d.IdDatabase == id);

            if (database == null)
                throw new Exception($"База данных с ID {id} не найдена");

            // Проверяем наличие связанных запросов
            if (database.Queries.Any())
            {
                throw new Exception(
                    $"Невозможно удалить базу данных '{database.Name}'.\n" +
                    $"Сначала удалите связанные запросы ({database.Queries.Count} шт.)"
                );
            }

            _context.Databases.Remove(database);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Проверка существования БД по имени
        /// </summary>
        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Databases.AnyAsync(d => d.Name == name);
        }
    }
}