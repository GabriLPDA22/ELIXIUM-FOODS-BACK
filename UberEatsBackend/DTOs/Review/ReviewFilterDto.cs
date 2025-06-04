// UberEatsBackend/DTOs/Review/ReviewFilterDto.cs
namespace UberEatsBackend.DTOs.Review
{
    public class ReviewFilterDto
    {
        public int? Rating { get; set; } // Filtrar por rating específico
        public bool? VerifiedOnly { get; set; } // Solo compras verificadas
        public bool? WithImages { get; set; } // Solo reseñas con imágenes
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SortBy { get; set; } = "newest"; // newest, oldest, highest_rating, lowest_rating, most_helpful
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}