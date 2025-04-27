namespace UberEatsBackend.DTOs.Restaurant
{
    public class UpdateRestaurantDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string CoverImageUrl { get; set; }
        public bool IsOpen { get; set; }
        public decimal DeliveryFee { get; set; }
        public int EstimatedDeliveryTime { get; set; }
    }
}