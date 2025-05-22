using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.RestaurantProduct;

namespace UberEatsBackend.Services
{
  public interface IRestaurantProductService
  {
    Task<List<RestaurantProductDto>> GetRestaurantProductsAsync(int restaurantId);
    Task<RestaurantProductDto?> GetRestaurantProductAsync(int restaurantId, int productId);
    Task<RestaurantProductDto> AssignProductToRestaurantAsync(int restaurantId, CreateRestaurantProductDto createDto);
    Task<RestaurantProductDto?> UpdateRestaurantProductAsync(int restaurantId, int productId, UpdateRestaurantProductDto updateDto);
    Task<bool> RemoveProductFromRestaurantAsync(int restaurantId, int productId);
    Task<List<RestaurantProductDto>> BulkAssignProductsAsync(int restaurantId, List<CreateRestaurantProductDto> products);
    Task<List<RestaurantProductDto>> GetProductsByBusinessAsync(int businessId);
  }
}
