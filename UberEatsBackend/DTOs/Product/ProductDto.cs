namespace UberEatsBackend.DTOs.Product
{
  public class ProductDto
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal BasePrice { get; set; }
    public string ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int BusinessId { get; set; }
    public string? BusinessName { get; set; }
    public int? RestaurantId { get; set; }
    public string? RestaurantName { get; set; }
    public decimal? RestaurantPrice { get; set; }
    public bool? RestaurantProductIsAvailable { get; set; }
    public int? StockQuantity { get; set; }
  }
}
