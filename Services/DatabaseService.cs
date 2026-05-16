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
        /// Создать новую базу данных с изображением (Base64)
        /// </summary>
        public async Task<Database> CreateAsync(string name, string imagePath = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя базы данных не может быть пустым");

            bool exists = await _context.Databases.AnyAsync(d => d.Name == name);
            if (exists)
                throw new Exception($"База данных с именем '{name}' уже существует");

            // Конвертируем изображение в Base64
            string imageBase64 = null;
            if (!string.IsNullOrEmpty(imagePath))
            {
                imageBase64 = ImageService.ImageToBase64(imagePath);
            }

            var database = new Database
            {
                Name = name.Trim(),
                SchemaImage = imageBase64
            };

            _context.Databases.Add(database);
            await _context.SaveChangesAsync();

            return database;
        }

        /// <summary>
        /// Обновить базу данных с новым изображением
        /// </summary>
        public async Task<Database> UpdateAsync(int id, string newName, string imagePath = null)
        {
            var database = await _context.Databases.FindAsync(id);
            if (database == null)
                throw new Exception($"База данных с ID {id} не найдена");

            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Имя базы данных не может быть пустым");

            database.Name = newName.Trim();

            // Если передано новое изображение, конвертируем его
            if (!string.IsNullOrEmpty(imagePath))
            {
                database.SchemaImage = ImageService.ImageToBase64(imagePath);
            }

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
        /// Удалить базу данных
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var database = await _context.Databases
                .Include(d => d.Queries)
                .FirstOrDefaultAsync(d => d.IdDatabase == id);

            if (database == null)
                throw new Exception($"База данных с ID {id} не найдена");

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