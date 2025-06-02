using System;
using System.ComponentModel.DataAnnotations;

namespace UberEatsBackend.DTOs.Offers
{
    public class AppliedOfferDto
    {
        public int OfferId { get; set; }
        public string OfferName { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
