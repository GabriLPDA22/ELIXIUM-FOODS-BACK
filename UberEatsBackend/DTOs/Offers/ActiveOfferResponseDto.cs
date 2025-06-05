// CREAR: UberEatsBackend/DTOs/Offers/ActiveOfferResponseDto.cs

namespace UberEatsBackend.DTOs.Offers
{
    public class ActiveOfferResponseDto
    {
        public int OfferId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string DiscountType { get; set; } = string.Empty; // "percentage" o "fixed"
        public decimal DiscountValue { get; set; }
        public decimal MinimumOrderAmount { get; set; }
        public int MinimumQuantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? RemainingUses { get; set; } // null si es ilimitado
    }
}
