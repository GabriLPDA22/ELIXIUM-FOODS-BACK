using System;
using System.ComponentModel.DataAnnotations;

namespace UberEatsBackend.DTOs.Offers
{
    public class CreateProductOfferDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }

        [Required]
        [RegularExpression("^(percentage|fixed)$")]
        public string DiscountType { get; set; } = "percentage";

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal DiscountValue { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinimumOrderAmount { get; set; } = 0;

        [Range(1, int.MaxValue)]
        public int MinimumQuantity { get; set; } = 1;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(0, int.MaxValue)]
        public int UsageLimit { get; set; } = 0; // 0 = sin límite
    }
}
