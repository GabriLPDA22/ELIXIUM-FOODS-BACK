using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.Models;

namespace UberEatsBackend.Services
{
  public interface IRestaurantService
  {
    Task<List<Restaurant>> GetAllRestaurantsAsync(); // <--- AÑADE ESTA LÍNEA
    Task<List<Restaurant>> GetPopularRestaurantsAsync(int limit = 10);
    Task<List<Restaurant>> SearchRestaurantsAsync(string? query, string? cuisine);
    Task<Restaurant?> GetRestaurantWithDetailsAsync(int id);
    Task<bool> IsUserAuthorizedForRestaurant(int restaurantId, int userId, string userRole);
    Task<List<Restaurant>> GetRestaurantsByTipoAsync(int tipo);
    Task<List<Restaurant>> GetRestaurantsByBusinessIdAsync(int businessId);
    Task<bool> IsBusinessOwner(int businessId, int userId);
    Task<List<Restaurant>> GetRestaurantsForAdminAsync(int userId);
  }
}
