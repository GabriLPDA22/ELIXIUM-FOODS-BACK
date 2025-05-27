namespace UberEatsBackend.DTOs.Product
{
  public class ProductDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;

    // Información específica del restaurante (si aplica)
    public decimal? RestaurantPrice { get; set; }
    public bool? RestaurantAvailability { get; set; }
    public int? StockQuantity { get; set; }
  }
}
