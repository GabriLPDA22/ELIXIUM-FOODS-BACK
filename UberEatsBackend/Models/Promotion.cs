using System;

namespace UberEatsBackend.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // discount, welcome, seasonal, loyalty
        public string DiscountType { get; set; } = "percentage"; // percentage, fixed
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal MinimumOrderValue { get; set; }
        public int UsageLimit { get; set; }
        public int UsageCount { get; set; }
        public string Status { get; set; } = "active"; // active, inactive, scheduled, expired

        // Relación con Business
        public int BusinessId { get; set; }
        public Business Business { get; set; } = null!;
    }
}
