using System;

namespace UberEatsBackend.DTOs.Offers
{
    public class ProductOfferSummaryDto
    {
        public int OfferId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal OriginalPrice { get; set; }
        public bool Applied { get; set; }
        public decimal CalculatedDiscount { get; set; }
        public decimal FinalPrice { get; set; }
        public string? ReasonNotApplied { get; set; }
    }
}
