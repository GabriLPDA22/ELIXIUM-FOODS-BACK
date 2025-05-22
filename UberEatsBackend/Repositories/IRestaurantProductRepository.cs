using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
  public interface IRestaurantProductRepository : IRepository<RestaurantProduct>
  {
    Task<List<RestaurantProduct>> GetByRestaurantIdAsync(int restaurantId);
    Task<List<RestaurantProduct>> GetByProductIdAsync(int productId);
    Task<RestaurantProduct?> GetByRestaurantAndProductAsync(int restaurantId, int productId);
    Task<List<RestaurantProduct>> GetByBusinessIdAsync(int businessId);
    Task<bool> ExistsAsync(int restaurantId, int productId);
    Task BulkInsertAsync(List<RestaurantProduct> restaurantProducts);
    Task BulkUpdateAsync(List<RestaurantProduct> restaurantProducts);
  }
}
