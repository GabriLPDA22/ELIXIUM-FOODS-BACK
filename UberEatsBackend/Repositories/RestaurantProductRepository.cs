using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
  public class RestaurantProductRepository : Repository<RestaurantProduct>, IRestaurantProductRepository
  {
    public RestaurantProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<RestaurantProduct>> GetByRestaurantIdAsync(int restaurantId)
    {
      return await _context.RestaurantProducts
          .Include(rp => rp.Product)
              .ThenInclude(p => p.Category)
          .Include(rp => rp.Restaurant)
          .Where(rp => rp.RestaurantId == restaurantId)
          .ToListAsync();
    }

    public async Task<List<RestaurantProduct>> GetByProductIdAsync(int productId)
    {
      return await _context.RestaurantProducts
          .Include(rp => rp.Restaurant)
          .Include(rp => rp.Product)
          .Where(rp => rp.ProductId == productId)
          .ToListAsync();
    }

    public async Task<RestaurantProduct?> GetByRestaurantAndProductAsync(int restaurantId, int productId)
    {
      return await _context.RestaurantProducts
          .Include(rp => rp.Product)
              .ThenInclude(p => p.Category)
          .Include(rp => rp.Restaurant)
          .FirstOrDefaultAsync(rp => rp.RestaurantId == restaurantId && rp.ProductId == productId);
    }

    public async Task<List<RestaurantProduct>> GetByBusinessIdAsync(int businessId)
    {
      return await _context.RestaurantProducts
          .Include(rp => rp.Product)
              .ThenInclude(p => p.Category)
          .Include(rp => rp.Restaurant)
          .Where(rp => rp.Restaurant.BusinessId == businessId)
          .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int restaurantId, int productId)
    {
      return await _context.RestaurantProducts
          .AnyAsync(rp => rp.RestaurantId == restaurantId && rp.ProductId == productId);
    }

    public async Task BulkInsertAsync(List<RestaurantProduct> restaurantProducts)
    {
      await _context.RestaurantProducts.AddRangeAsync(restaurantProducts);
      await _context.SaveChangesAsync();
    }

    public async Task BulkUpdateAsync(List<RestaurantProduct> restaurantProducts)
    {
      _context.RestaurantProducts.UpdateRange(restaurantProducts);
      await _context.SaveChangesAsync();
    }

    public override async Task<List<RestaurantProduct>> GetAllAsync()
    {
      return await _context.RestaurantProducts
          .Include(rp => rp.Product)
              .ThenInclude(p => p.Category)
          .Include(rp => rp.Restaurant)
          .ToListAsync();
    }
  }
}
