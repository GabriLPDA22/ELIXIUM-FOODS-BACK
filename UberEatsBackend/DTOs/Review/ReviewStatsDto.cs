// UberEatsBackend/DTOs/Review/ReviewStatsDto.cs
namespace UberEatsBackend.DTOs.Review
{
    public class ReviewStatsDto
    {
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public int RecentReviews { get; set; } // Últimos 30 días
    }
    
    public class RestaurantReviewSummaryDto
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public ReviewStatsDto Stats { get; set; } = new();
        public List<ReviewDto> RecentReviews { get; set; } = new();
    }
    
    public class ProductReviewSummaryDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public ReviewStatsDto Stats { get; set; } = new();
        public List<ReviewDto> RecentReviews { get; set; } = new();
    }
}
