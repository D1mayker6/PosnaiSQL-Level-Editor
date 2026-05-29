using Microsoft.EntityFrameworkCore;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Entities;

namespace PosnaiSQLauncher.Services
{
    public class QueryService
    {
        private readonly AppDbContext _context;

        public QueryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Query> CreateAsync(int databaseId, string name, string condition, string queryString)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя запроса не может быть пустым");

            bool databaseExists = await _context.Databases.AnyAsync(d => d.IdDatabase == databaseId);
            if (!databaseExists)
                throw new Exception($"База данных с ID {databaseId} не найдена");

            var query = new Query
            {
                IdDatabase = databaseId,
                Name = name.Trim(),
                Condition = condition?.Trim(),
                QueryString = queryString?.Trim()
            };

            _context.Queries.Add(query);
            await _context.SaveChangesAsync();

            return query;
        }

        public async Task<List<Query>> GetByDatabaseIdAsync(int databaseId)
        {
            return await _context.Queries
                .Where(q => q.IdDatabase == databaseId)
                .OrderBy(q => q.Name)
                .ToListAsync();
        }

        public async Task<Query> GetByIdAsync(int id)
        {
            var query = await _context.Queries
                .Include(q => q.IdDatabaseNavigation)
                .FirstOrDefaultAsync(q => q.IdQuery == id);

            if (query == null)
                throw new Exception($"Запрос с ID {id} не найден");

            return query;
        }

        public async Task<Query> UpdateAsync(int id, string name, string condition, string queryString)
        {
            var query = await _context.Queries.FindAsync(id);
            if (query == null)
                throw new Exception($"Запрос с ID {id} не найден");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя запроса не может быть пустым");

            query.Name = name.Trim();
            query.Condition = condition?.Trim();
            query.QueryString = queryString?.Trim();

            await _context.SaveChangesAsync();

            return query;
        }
        
        public async Task DeleteAsync(int id)
        {
            var query = await _context.Queries
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.IdQuery == id);

            if (query == null)
                throw new Exception($"Запрос с ID {id} не найден");

            if (query.Options.Any())
            {
                throw new Exception(
                    $"Невозможно удалить запрос '{query.Name}'.\n" +
                    $"Сначала удалите связанные варианты ({query.Options.Count} шт.)"
                );
            }

            _context.Queries.Remove(query);
            await _context.SaveChangesAsync();
        }
        
        public async Task<List<Query>> GetAllAsync()
        {
            return await _context.Queries
                .Include(q => q.IdDatabaseNavigation)
                .OrderBy(q => q.Name)
                .ToListAsync();
        }
    }
}