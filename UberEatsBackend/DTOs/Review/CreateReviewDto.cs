// UberEatsBackend/DTOs/Review/CreateReviewDto.cs
namespace UberEatsBackend.DTOs.Review
{
    public class CreateReviewDto
    {
        public int RestaurantId { get; set; }
        public int? ProductId { get; set; } // Opcional - null para reseña de restaurante
        public int Rating { get; set; } // 1-5 estrellas
        public string Comment { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}