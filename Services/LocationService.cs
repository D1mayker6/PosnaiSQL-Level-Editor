using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        /// <summary>
        /// Получить все локации
        /// </summary>
        public async Task<List<Location>> GetAllAsync()
        {
            return await _context.Locations
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Получить локацию по ID
        /// </summary>
        public async Task<Location> GetByIdAsync(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
                throw new Exception($"Локация с ID {id} не найдена");

            return location;
        }
    }
}