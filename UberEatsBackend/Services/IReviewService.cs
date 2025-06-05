// UberEatsBackend/Services/IReviewService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Review;

namespace UberEatsBackend.Services
{
    public interface IReviewService
    {
        Task<List<ReviewDto>> GetRestaurantReviewsAsync(int restaurantId, ReviewFilterDto? filter = null);
        Task<List<ReviewDto>> GetProductReviewsAsync(int productId, ReviewFilterDto? filter = null);
        Task<List<ReviewDto>> GetUserReviewsAsync(int userId, ReviewFilterDto? filter = null);
        Task<ReviewDto?> GetReviewByIdAsync(int id);
        Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewDto createDto);
        Task<ReviewDto?> UpdateReviewAsync(int id, int userId, UpdateReviewDto updateDto);
        Task<bool> DeleteReviewAsync(int id, int userId, string userRole);
        Task<RestaurantReviewSummaryDto?> GetRestaurantReviewSummaryAsync(int restaurantId);
        Task<ProductReviewSummaryDto?> GetProductReviewSummaryAsync(int productId);
        Task<bool> CanUserReviewRestaurantAsync(int userId, int restaurantId);
        Task<bool> CanUserReviewProductAsync(int userId, int productId);
        Task<bool> MarkReviewAsHelpfulAsync(int reviewId, int userId);
        Task<ReviewDto?> GetUserReviewForRestaurantAsync(int userId, int restaurantId);
        Task<ReviewDto?> GetUserReviewForProductAsync(int userId, int productId);
    }
}