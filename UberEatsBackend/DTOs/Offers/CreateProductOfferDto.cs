using System;
using System.ComponentModel.DataAnnotations;

namespace UberEatsBackend.DTOs.Offers
{
  public class CreateProductOfferDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = "percentage"; // "percentage" o "fixed"
        public decimal DiscountValue { get; set; }
        public decimal MinimumOrderAmount { get; set; } = 0;
        public int MinimumQuantity { get; set; } = 1;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsageLimit { get; set; } = 0; // 0 = sin límite
        public int ProductId { get; set; }
    }
}
