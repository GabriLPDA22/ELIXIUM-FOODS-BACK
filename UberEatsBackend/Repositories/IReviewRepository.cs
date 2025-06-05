// UberEatsBackend/Repositories/IReviewRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.Models;
using UberEatsBackend.DTOs.Review;

namespace UberEatsBackend.Repositories
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<List<Review>> GetReviewsByRestaurantIdAsync(int restaurantId, ReviewFilterDto? filter = null);
        Task<List<Review>> GetReviewsByProductIdAsync(int productId, ReviewFilterDto? filter = null);
        Task<List<Review>> GetReviewsByUserIdAsync(int userId, ReviewFilterDto? filter = null);
        Task<Review?> GetUserReviewForRestaurantAsync(int userId, int restaurantId);
        Task<Review?> GetUserReviewForProductAsync(int userId, int productId);
        Task<ReviewStatsDto> GetRestaurantStatsAsync(int restaurantId);
        Task<ReviewStatsDto> GetProductStatsAsync(int productId);
        Task<bool> HasUserPurchasedProductAsync(int userId, int productId);
        Task<bool> HasUserOrderedFromRestaurantAsync(int userId, int restaurantId);
        Task<List<Review>> GetTopReviewsByRestaurantAsync(int restaurantId, int limit = 5);
        Task<List<Review>> GetTopReviewsByProductAsync(int productId, int limit = 5);
        Task<double> CalculateAverageRatingAsync(int restaurantId, int? productId = null);
        Task UpdateRestaurantAverageRatingAsync(int restaurantId);
    }
}
