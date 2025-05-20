using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
  public class RestaurantRepository : Repository<Restaurant>, IRestaurantRepository
  {
    public RestaurantRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Restaurant?> GetWithDetailsAsync(int id)
    {
      return await _context.Restaurants
          .Include(r => r.Address)
          .Include(r => r.Menus)
              .ThenInclude(m => m.Categories)
                  .ThenInclude(c => c.Products)
          .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Restaurant>> GetPopularRestaurantsAsync(int limit = 10)
    {
      return await _context.Restaurants
          .Include(r => r.Address)
          .OrderByDescending(r => r.AverageRating)
          .Take(limit)
          .ToListAsync();
    }

    public async Task<List<Restaurant>> SearchRestaurantsAsync(string? query, string? cuisine)
    {
      var queryable = _context.Restaurants
          .Include(r => r.Address)
          .AsQueryable();

      if (!string.IsNullOrEmpty(query))
      {
        queryable = queryable.Where(r =>
            r.Name.Contains(query) ||
            r.Description.Contains(query));
      }

      if (!string.IsNullOrEmpty(cuisine))
      {
        queryable = queryable.Where(r =>
            r.Description.Contains(cuisine)); // Simplificado - podrías agregar un campo de cocina
      }

      return await queryable.OrderBy(r => r.Name).ToListAsync();
    }

    public async Task<List<Restaurant>> GetByTipoAsync(int tipo)
    {
      return await _context.Restaurants
          .Include(r => r.Address)
          .Where(r => r.Tipo == tipo)
          .OrderBy(r => r.Name)
          .ToListAsync();
    }

    // Método para obtener restaurantes por business ID
    public async Task<List<Restaurant>> GetByBusinessIdAsync(int businessId)
    {
      return await _context.Restaurants
          .Include(r => r.Address)
          .Where(r => r.BusinessId == businessId)
          .OrderBy(r => r.Name)
          .ToListAsync();
    }
  }
}
