// Updated RestaurantDto.cs
using System;
using UberEatsBackend.DTOs.Address;

namespace UberEatsBackend.DTOs.Restaurant
{
    public class RestaurantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public bool IsOpen { get; set; }
        public decimal DeliveryFee { get; set; }
        public int EstimatedDeliveryTime { get; set; }
        public AddressDto Address { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Tipo { get; set; }

        // Business reference fields
        public int? BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
    }
}
