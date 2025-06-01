using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
  public class ProductRepository : Repository<Product>, IProductRepository
  {
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Product>> GetProductsByBusinessIdAsync(int businessId)
    {
      return await _context.Products
          .Include(p => p.Category)
          .Where(p => p.Category != null && p.Category.BusinessId == businessId)
          .ToListAsync();
    }

    public async Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId)
    {
      return await _context.Products
          .Include(p => p.Category)
          .Where(p => p.CategoryId == categoryId)
          .ToListAsync();
    }

    public async Task<Product?> GetProductWithDetailsAsync(int productId)
    {
      return await _context.Products
          .Include(p => p.Category)
              .ThenInclude(c => c!.Business)
          .FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task<List<Product>> GetProductsByRestaurantIdAsync(int restaurantId)
    {
      return await _context.RestaurantProducts
          .Where(rp => rp.RestaurantId == restaurantId && rp.Product != null)
          .Include(rp => rp.Product)
              .ThenInclude(p => p!.Category)
          .Select(rp => rp.Product!)
          .ToListAsync();
    }

    public async Task<List<Product>> SearchAsync(string query, int? categoryId)
    {
        var queryable = _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c!.Business)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            queryable = queryable.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            string lowerQuery = query.ToLower();
            queryable = queryable.Where(p =>
                (p.Name != null && p.Name.ToLower().Contains(lowerQuery)) ||
                (p.Description != null && p.Description.ToLower().Contains(lowerQuery))
            );
        }
        return await queryable.Distinct().ToListAsync();
    }

    public override async Task<List<Product>> GetAllAsync()
    {
      return await _context.Products
          .Include(p => p.Category)
              .ThenInclude(c => c!.Business)
          .ToListAsync();
    }
  }
}
