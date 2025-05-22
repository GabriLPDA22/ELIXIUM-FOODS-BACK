using System;

namespace UberEatsBackend.Models
{
  public class RestaurantProduct
  {
    public int Id { get; set; }

    // Relaciones
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Campos específicos por restaurante
    public decimal Price { get; set; } // Precio específico en este restaurante
    public bool IsAvailable { get; set; } = true; // Disponibilidad en este restaurante
    public int StockQuantity { get; set; } = 0; // Stock opcional
    public string? Notes { get; set; } // Notas específicas del restaurante

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

    public RestaurantProduct()
    {
      CreatedAt = DateTime.UtcNow;
      UpdatedAt = DateTime.UtcNow;
    }
  }
}
