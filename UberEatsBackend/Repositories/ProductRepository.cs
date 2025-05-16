using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
  public interface IProductRepository : IRepository<Product>
  {
    Task<List<Product>> GetProductsByRestaurantIdAsync(int restaurantId);
    Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId);
    Task<Product?> GetProductWithDetailsAsync(int productId);
  }

  public class ProductRepository : Repository<Product>, IProductRepository
  {
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Product>> GetProductsByRestaurantIdAsync(int restaurantId)
    {
      // Obtener todos los menús del restaurante
      var menuIds = await _context.Menus
          .Where(m => m.RestaurantId == restaurantId)
          .Select(m => m.Id)
          .ToListAsync();

      // Obtener todas las categorías de esos menús
      var categoryIds = await _context.Categories
          .Where(c => menuIds.Contains(c.MenuId ?? 0))
          .Select(c => c.Id)
          .ToListAsync();

      // Obtener todos los productos de esas categorías
      return await _context.Products
          .Where(p => categoryIds.Contains(p.CategoryId))
          .ToListAsync();
    }

    public async Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId)
    {
      return await _context.Products
          .Where(p => p.CategoryId == categoryId)
          .ToListAsync();
    }

    public async Task<Product?> GetProductWithDetailsAsync(int productId)
    {
      return await _context.Products
          .Include(p => p.Category)
              .ThenInclude(c => c.Menu)
                  .ThenInclude(m => m.Restaurant)
          .FirstOrDefaultAsync(p => p.Id == productId);
    }

    // Sobrescribir GetAllAsync para incluir detalles
    public override async Task<List<Product>> GetAllAsync()
    {
      return await _context.Products
          .Include(p => p.Category)
          .ToListAsync();
    }
  }
}