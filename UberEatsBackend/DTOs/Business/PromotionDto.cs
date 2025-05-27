// UberEatsBackend/DTOs/Business/PromotionDto.cs
namespace UberEatsBackend.DTOs.Business
{
  public class PromotionDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal MinimumOrderValue { get; set; }
    public int UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
  }

  public class CreatePromotionDto
  {
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string DiscountType { get; set; } = "percentage";
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal MinimumOrderValue { get; set; }
    public int UsageLimit { get; set; }
    public string Status { get; set; } = "active";
  }

public class UpdatePromotionDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Code { get; set; }
        public decimal? MinimumOrderValue { get; set; }
        public int? UsageLimit { get; set; }
    }
}
