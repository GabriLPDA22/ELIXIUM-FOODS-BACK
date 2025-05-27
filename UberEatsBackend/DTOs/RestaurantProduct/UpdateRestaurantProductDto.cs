namespace UberEatsBackend.DTOs.RestaurantProduct
{
    public class UpdateRestaurantProductDto
    {
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public int StockQuantity { get; set; }
        public string? Notes { get; set; }
    }
}
