using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UberEatsBackend.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "RestaurantId must be a positive number")]
        public int RestaurantId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "DeliveryAddressId must be a positive number")]
        public int DeliveryAddressId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "card";
    }

    public class CreateOrderItemDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive number")]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }
    }
}
