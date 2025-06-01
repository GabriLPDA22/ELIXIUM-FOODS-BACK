// UberEatsBackend/Services/IRestaurantHourService.cs
using UberEatsBackend.DTOs.Restaurant;
using UberEatsBackend.Models;

namespace UberEatsBackend.Services
{
    public interface IRestaurantHourService
    {
        Task<List<RestaurantHourDto>> GetRestaurantHoursAsync(int restaurantId);
        Task<bool> BulkUpdateRestaurantHoursAsync(int restaurantId, BulkUpdateRestaurantHoursDto hoursDto);
        Task<bool> IsRestaurantOpenAsync(int restaurantId);
        Task<bool> IsRestaurantOpenAtTimeAsync(int restaurantId, DateTime dateTime);
        Task<string> GetRestaurantStatusAsync(int restaurantId);
    }
}
