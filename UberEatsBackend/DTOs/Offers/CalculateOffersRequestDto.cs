// CREAR: UberEatsBackend/DTOs/Offers/CalculateOffersRequestDto.cs

namespace UberEatsBackend.DTOs.Offers
{
    public class CalculateOffersRequestDto
    {
        public List<ProductForCalculationDto> Products { get; set; } = new List<ProductForCalculationDto>();
        public decimal OrderSubtotal { get; set; }
    }

    public class ProductForCalculationDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
