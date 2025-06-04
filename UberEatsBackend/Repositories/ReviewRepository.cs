// UberEatsBackend/Repositories/ReviewRepository.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.Models;
using UberEatsBackend.DTOs.Review;

namespace UberEatsBackend.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Review>> GetReviewsByRestaurantIdAsync(int restaurantId, ReviewFilterDto? filter = null)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .Include(r => r.Product)
                .Where(r => r.RestaurantId == restaurantId && r.IsActive);

            if (filter != null)
            {
                query = ApplyFilters(query, filter);
            }

            return await query.ToListAsync();
        }

        public async Task<List<Review>> GetReviewsByProductIdAsync(int productId, ReviewFilterDto? filter = null)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId && r.IsActive);

            if (filter != null)
            {
                query = ApplyFilters(query, filter);
            }

            return await query.ToListAsync();
        }

        public async Task<List<Review>> GetReviewsByUserIdAsync(int userId, ReviewFilterDto? filter = null)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .Include(r => r.Product)
                .Where(r => r.UserId == userId && r.IsActive);

            if (filter != null)
            {
                query = ApplyFilters(query, filter);
            }

            return await query.ToListAsync();
        }

        public async Task<Review?> GetUserReviewForRestaurantAsync(int userId, int restaurantId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.UserId == userId && 
                                        r.RestaurantId == restaurantId && 
                                        r.ProductId == null && 
                                        r.IsActive);
        }

        public async Task<Review?> GetUserReviewForProductAsync(int userId, int productId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.UserId == userId && 
                                        r.ProductId == productId && 
                                        r.IsActive);
        }

        public async Task<ReviewStatsDto> GetRestaurantStatsAsync(int restaurantId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.RestaurantId == restaurantId && r.IsActive)
                .ToListAsync();

            return CalculateStats(reviews);
        }

        public async Task<ReviewStatsDto> GetProductStatsAsync(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsActive)
                .ToListAsync();

            return CalculateStats(reviews);
        }

        public async Task<bool> HasUserPurchasedProductAsync(int userId, int productId)
        {
            // Verificar si el usuario ha ordenado este producto
            return await _context.OrderItems
                .AnyAsync(oi => oi.Order.UserId == userId && 
                               oi.ProductId == productId && 
                               oi.Order.Status == "Completed");
        }

        public async Task<bool> HasUserOrderedFromRestaurantAsync(int userId, int restaurantId)
        {
            // Verificar si el usuario ha ordenado de este restaurante
            return await _context.Orders
                .AnyAsync(o => o.UserId == userId && 
                              o.RestaurantId == restaurantId && 
                              o.Status == "Completed");
        }

        public async Task<List<Review>> GetTopReviewsByRestaurantAsync(int restaurantId, int limit = 5)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .Include(r => r.Product)
                .Where(r => r.RestaurantId == restaurantId && r.IsActive)
                .OrderByDescending(r => r.HelpfulCount)
                .ThenByDescending(r => r.Rating)
                .ThenByDescending(r => r.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Review>> GetTopReviewsByProductAsync(int productId, int limit = 5)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId && r.IsActive)
                .OrderByDescending(r => r.HelpfulCount)
                .ThenByDescending(r => r.Rating)
                .ThenByDescending(r => r.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<double> CalculateAverageRatingAsync(int restaurantId, int? productId = null)
        {
            var query = _context.Reviews.Where(r => r.RestaurantId == restaurantId && r.IsActive);
            
            if (productId.HasValue)
            {
                query = query.Where(r => r.ProductId == productId.Value);
            }

            var ratings = await query.Select(r => r.Rating).ToListAsync();
            return ratings.Any() ? ratings.Average() : 0;
        }

        public async Task UpdateRestaurantAverageRatingAsync(int restaurantId)
        {
            var avgRating = await CalculateAverageRatingAsync(restaurantId);
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            
            if (restaurant != null)
            {
                restaurant.AverageRating = avgRating;
                restaurant.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private IQueryable<Review> ApplyFilters(IQueryable<Review> query, ReviewFilterDto filter)
        {
            if (filter.Rating.HasValue)
            {
                query = query.Where(r => r.Rating == filter.Rating.Value);
            }

            if (filter.VerifiedOnly == true)
            {
                query = query.Where(r => r.IsVerifiedPurchase);
            }

            if (filter.WithImages == true)
            {
                query = query.Where(r => !string.IsNullOrEmpty(r.ImageUrl));
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= filter.DateTo.Value);
            }

            // Aplicar ordenamiento
            query = filter.SortBy?.ToLower() switch
            {
                "oldest" => query.OrderBy(r => r.CreatedAt),
                "highest_rating" => query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                "lowest_rating" => query.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                "most_helpful" => query.OrderByDescending(r => r.HelpfulCount).ThenByDescending(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt) // newest por defecto
            };

            // Aplicar paginación
            var skip = (filter.Page - 1) * filter.PageSize;
            query = query.Skip(skip).Take(filter.PageSize);

            return query;
        }

        private ReviewStatsDto CalculateStats(List<Review> reviews)
        {
            var stats = new ReviewStatsDto
            {
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                RecentReviews = reviews.Count(r => r.CreatedAt >= DateTime.UtcNow.AddDays(-30))
            };

            // Distribución de ratings
            for (int i = 1; i <= 5; i++)
            {
                stats.RatingDistribution[i] = reviews.Count(r => r.Rating == i);
            }

            return stats;
        }

        public override async Task<List<Review>> GetAllAsync()
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .Include(r => r.Product)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
