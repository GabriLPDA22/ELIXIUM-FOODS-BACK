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

        // ✅ MANTENER: PaymentMethodId para crear el Payment, luego se asigna PaymentId al Order
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PaymentMethodId must be a positive number")]
        public int PaymentMethodId { get; set; }

        // Campos opcionales para delivery programado
        public string? DeliveryType { get; set; }
        public string? ScheduledDate { get; set; }
        public string? DeliveryInstructions { get; set; }
    }

    public class CreateOrderItemDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive number")]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0")]
        public decimal UnitPrice { get; set; }
    }
}
