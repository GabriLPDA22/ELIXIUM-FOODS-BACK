using System.Collections.Generic;

namespace UberEatsBackend.Models
{
  public class Address
  {
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Relaciones
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Navigation properties
    public List<Order> Orders { get; set; } = new List<Order>();
    public Restaurant? Restaurant { get; set; }
  }
}
