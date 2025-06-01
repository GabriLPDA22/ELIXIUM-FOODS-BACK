using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.Models;
using UberEatsBackend.DTOs.Restaurant;

namespace UberEatsBackend.Services
{
  public interface IRestaurantService
  {
    Task<List<Restaurant>> GetAllRestaurantsAsync();
    Task<List<Restaurant>> GetPopularRestaurantsAsync(int limit = 10);
    Task<List<Restaurant>> SearchRestaurantsAsync(string query, int? categoryId);
    Task<Restaurant?> GetRestaurantWithDetailsAsync(int id);
    Task<bool> IsUserAuthorizedForRestaurant(int restaurantId, int userId, string userRole);
    Task<List<Restaurant>> GetRestaurantsByTipoAsync(int tipo);
    Task<List<Restaurant>> GetRestaurantsByBusinessIdAsync(int businessId);
    Task<bool> IsBusinessOwner(int businessId, int userId);
    Task<List<Restaurant>> GetRestaurantsForAdminAsync(int userId);
    Task<List<RestaurantCardWithStatusDto>> GetAllRestaurantsWithStatusAsync();
    Task<RestaurantDetailWithStatusDto?> GetRestaurantWithStatusAsync(int id);
    Task<bool> IsRestaurantCurrentlyOpenAsync(int restaurantId);
    Task<string> GetRestaurantStatusMessageAsync(int restaurantId);
  }
}
