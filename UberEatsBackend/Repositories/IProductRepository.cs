using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
  public interface IProductRepository : IRepository<Product>
  {
    Task<List<Product>> GetProductsByBusinessIdAsync(int businessId);
    Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId);
    Task<Product?> GetProductWithDetailsAsync(int productId);
    Task<List<Product>> GetProductsByRestaurantIdAsync(int restaurantId);
  }
}
