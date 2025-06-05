namespace UberEatsBackend.DTOs.RestaurantProduct
{
    public class RestaurantProductOfferingDto
    {
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public string? RestaurantLogoUrl { get; set; } // <-- AÑADIR ESTO
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public int? StockQuantity { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
    }
}
