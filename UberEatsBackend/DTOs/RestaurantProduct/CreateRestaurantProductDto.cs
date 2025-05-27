namespace UberEatsBackend.DTOs.RestaurantProduct
{
    public class CreateRestaurantProductDto
    {
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int StockQuantity { get; set; } = 0;
        public string? Notes { get; set; }
    }
}
