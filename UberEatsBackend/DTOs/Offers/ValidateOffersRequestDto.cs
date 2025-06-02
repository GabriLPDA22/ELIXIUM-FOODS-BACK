//  UberEatsBackend/DTOs/Offers/ValidateOffersRequestDto.cs

namespace UberEatsBackend.DTOs.Offers
{
    public class ValidateOffersRequestDto
    {
        public List<OrderItemForValidationDto> Items { get; set; } = new List<OrderItemForValidationDto>();
        public decimal OrderSubtotal { get; set; }
    }

    public class OrderItemForValidationDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
