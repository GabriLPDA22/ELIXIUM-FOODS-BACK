using System;
using System.Collections.Generic;

namespace UberEatsBackend.Models
{
  public class Restaurant
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public bool IsOpen { get; set; }
    public decimal DeliveryFee { get; set; }
    public int EstimatedDeliveryTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public int UserId { get; set; }
    public User Owner { get; set; } = null!;

    public int AddressId { get; set; }
    public Address Address { get; set; } = null!;

    // Navigation properties
    public List<Menu> Menus { get; set; } = new List<Menu>();
    public List<Order> Orders { get; set; } = new List<Order>();

    public Restaurant()
    {
      CreatedAt = DateTime.UtcNow;
      UpdatedAt = DateTime.UtcNow;
      AverageRating = 0;
    }
  }
}
