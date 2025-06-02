using System;
using System.ComponentModel.DataAnnotations;

namespace UberEatsBackend.DTOs.Offers
{
    public class UpdateProductOfferDto
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [RegularExpression("^(percentage|fixed)$")]
        public string? DiscountType { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? DiscountValue { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinimumOrderAmount { get; set; }

        [Range(1, int.MaxValue)]
        public int? MinimumQuantity { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(0, int.MaxValue)]
        public int? UsageLimit { get; set; }

        [RegularExpression("^(active|inactive)$")]
        public string? Status { get; set; }
    }
}
