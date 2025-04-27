using System.Collections.Generic;

namespace UberEatsBackend.Models
{
  public class Menu
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Relaciones
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    // Navigation properties
    public List<Category> Categories { get; set; } = new List<Category>();
  }
}
