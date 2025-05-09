using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
  public interface IRestaurantRepository : IRepository<Restaurant>
  {
    Task<Restaurant?> GetWithDetailsAsync(int id);
    Task<List<Restaurant>> GetPopularRestaurantsAsync(int limit = 10);
    Task<List<Restaurant>> SearchRestaurantsAsync(string? query, string? cuisine);
    Task<List<Restaurant>> GetByOwnerAsync(int userId);
    Task<bool> IsOwner(int restaurantId, int userId);
    Task<List<Restaurant>> GetByTipoAsync(int tipo);
  }
}