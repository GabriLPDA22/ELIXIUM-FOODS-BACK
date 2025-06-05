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
          .Include(r => r.Business)
          .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Restaurant>> GetPopularRestaurantsAsync(int limit = 10)
    {
      return await _context.Restaurants
          .Include(r => r.Address)
          .Include(r => r.Business)
          .OrderByDescending(r => r.AverageRating)
          .Take(limit)
          .ToListAsync();
    }

    public async Task<List<Restaurant>> SearchRestaurantsAsync(string query, int? categoryId)
    {
      var queryable = _context.Restaurants
          .Include(r => r.Address)
          .Include(r => r.Business)
          .AsQueryable();

      if (!string.IsNullOrWhiteSpace(query))
      {
        string lowerQuery = query.ToLower();
        queryable = queryable.Where(r =>
            (r.Name != null && r.Name.ToLower().Contains(lowerQuery)) ||
            (r.Description != null && r.Description.ToLower().Contains(lowerQuery)));
      }

      if (categoryId.HasValue)
      {
        // Assuming categoryId corresponds to the 'Tipo' field.
        // Adjust if it maps to a different concept or a related table.
        queryable = queryable.Where(r => r.Tipo == categoryId.Value);
      }

      return await queryable.OrderBy(r => r.Name).ToListAsync();
    }

    public async Task<List<Restaurant>> GetByTipoAsync(int tipo)
    {
      return await _context.Restaurants
          .Include(r => r.Address)
          .Include(r => r.Business)
          .Where(r => r.Tipo == tipo)
          .OrderBy(r => r.Name)
          .ToListAsync();
    }

    public async Task<List<Restaurant>> GetByBusinessIdAsync(int businessId)
    {
      return await _context.Restaurants
          .Include(r => r.Address)
          .Include(r => r.Business)
          .Where(r => r.BusinessId == businessId)
          .OrderBy(r => r.Name)
          .ToListAsync();
    }
  }
}
