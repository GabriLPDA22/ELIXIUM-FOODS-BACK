// UberEatsBackend/DTOs/Review/UpdateReviewDto.cs
namespace UberEatsBackend.DTOs.Review
{
    public class UpdateReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}