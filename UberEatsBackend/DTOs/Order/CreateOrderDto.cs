using System.Collections.Generic;

namespace UberEatsBackend.DTOs.Order
{
    public class CreateOrderDto
    {
        public int RestaurantId { get; set; }
        public int DeliveryAddressId { get; set; }
        public List<CreateOrderItemDto> Items { get; set; }
        public string PaymentMethod { get; set; } = "card";
    }

    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}