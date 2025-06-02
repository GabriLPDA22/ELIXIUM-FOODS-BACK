using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UberEatsBackend.DTOs.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public int DeliveryAddressId { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public int? DeliveryPersonId { get; set; }
        public string? DeliveryPersonName { get; set; }

        // ✅ ARREGLO: Añadir PaymentId
        public int? PaymentId { get; set; }

        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime EstimatedDeliveryTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();

        // ✅ MANTENER: Payment para mostrar detalles del pago
        public PaymentDto? Payment { get; set; }

        // ✅ NUEVO: Objetos completos para el frontend
        public OrderUserDto? User { get; set; }
        public OrderRestaurantDto? Restaurant { get; set; }
        public OrderAddressDto? DeliveryAddressDetails { get; set; }
        public OrderUserDto? DeliveryPerson { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        // ✅ NUEVO: Objeto completo del producto
        public OrderProductDto? Product { get; set; }
    }

    public class PaymentDto
    {
        public int Id { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    // ✅ NUEVO: DTOs específicos para Order
    public class OrderUserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public class OrderRestaurantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal DeliveryFee { get; set; }
        public int EstimatedDeliveryTime { get; set; }
        public bool IsOpen { get; set; }
    }

    public class OrderAddressDto
    {
        public int Id { get; set; }
        public string Street { get; set; } = string.Empty;
        public string? Number { get; set; }
        public string? Interior { get; set; }
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Phone { get; set; }
        public string? Notes { get; set; }
    }

    public class OrderProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal BasePrice { get; set; }
    }
}
