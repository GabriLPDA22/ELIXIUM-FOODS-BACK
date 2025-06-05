namespace UberEatsBackend.DTOs.Product
{
  public class CreateProductDto
  {
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public int CategoryId { get; set; }
    public int BusinessId { get; set; }
  }
}
