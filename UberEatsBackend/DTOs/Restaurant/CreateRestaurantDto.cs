using UberEatsBackend.DTOs.Address;

namespace UberEatsBackend.DTOs.Restaurant
{
    public class CreateRestaurantDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
        public decimal DeliveryFee { get; set; }
        public int EstimatedDeliveryTime { get; set; }
        public CreateAddressDto Address { get; set; } = null!;
        public int Tipo { get; set; } = 1;
    }
}
