//  UberEatsBackend/DTOs/Offers/OfferValidationDto.cs

namespace UberEatsBackend.DTOs.Offers
{
    public class OfferValidationDto
    {
        public int OfferId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public bool CanApply { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;
        public decimal? DiscountAmount { get; set; }
        public decimal? FinalPrice { get; set; }
    }
}
