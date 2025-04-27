using System.Collections.Generic;

namespace UberEatsBackend.Models
{
  public class Product
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;

    // Relaciones
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Navigation properties
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
  }
}
