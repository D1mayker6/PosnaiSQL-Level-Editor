using Microsoft.EntityFrameworkCore;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Entities;

namespace PosnaiSQLauncher.Services
{
    public class LocationService
    {
        private readonly AppDbContext _context;

        public LocationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Location>> GetAllAsync()
        {
            return await _context.Locations
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        public async Task<Location> GetByIdAsync(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
                throw new Exception($"Локация с ID {id} не найдена");

            return location;
        }
    }
}