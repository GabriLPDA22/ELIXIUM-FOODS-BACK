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
    public int Tipo { get; set; } = 1;

    // Relación con Business
    public int? BusinessId { get; set; }
    public Business? Business { get; set; }

    private DateTime _createdAt;
    private DateTime _updatedAt;

    public DateTime CreatedAt
    {
      get => _createdAt;
      set => _createdAt = value.Kind == DateTimeKind.Unspecified ?
          DateTime.SpecifyKind(value, DateTimeKind.Utc) :
          value.ToUniversalTime();
    }

    public DateTime UpdatedAt
    {
      get => _updatedAt;
      set => _updatedAt = value.Kind == DateTimeKind.Unspecified ?
          DateTime.SpecifyKind(value, DateTimeKind.Utc) :
          value.ToUniversalTime();
    }

    public int AddressId { get; set; }
    public Address Address { get; set; } = null!;

    // Navigation properties
    public List<Order> Orders { get; set; } = new List<Order>();
    public List<RestaurantProduct> RestaurantProducts { get; set; } = new List<RestaurantProduct>();

    public Restaurant()
    {
      CreatedAt = DateTime.UtcNow;
      UpdatedAt = DateTime.UtcNow;
      AverageRating = 0;
    }
  }
}
