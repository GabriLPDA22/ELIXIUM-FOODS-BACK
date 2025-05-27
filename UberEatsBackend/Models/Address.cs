using System.Collections.Generic;

namespace UberEatsBackend.Models
{
  public class Address
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Interior { get; set; } = string.Empty;
    public string? Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefault { get; set; } = false;

    // Relaciones - UserId ahora es nullable para soportar direcciones de restaurantes
    public int? UserId { get; set; }
    public User? User { get; set; }

    // Navigation properties
    public List<Order> Orders { get; set; } = new List<Order>();
    public Restaurant? Restaurant { get; set; }
  }
}
