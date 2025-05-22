using System.Collections.Generic;

namespace UberEatsBackend.Models
{
  public class Product
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; } // Precio base, puede variar por restaurante
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;

    // Pertenece a una categoría del business
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Propiedad de navegación calculada para obtener BusinessId
    public int BusinessId => Category?.BusinessId ?? 0;

    // Navigation properties
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public List<RestaurantProduct> RestaurantProducts { get; set; } = new List<RestaurantProduct>();
  }
}
