namespace UberEatsBackend.DTOs.Product
{
  public class UpdateProductDto
  {
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int CategoryId { get; set; }
  }
}
