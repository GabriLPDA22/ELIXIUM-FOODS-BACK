using System;

namespace UberEatsBackend.DTOs.Offers
{
  public class ProductOfferDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public int MinimumQuantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Propiedades calculadas
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public int RemainingUses { get; set; }
  }
  // DTOs adicionales para las requests del controlador
  public class ValidateOffersRequestDto
  {
    public List<OrderItemRequestDto> Items { get; set; } = new();
    public decimal OrderSubtotal { get; set; }
  }

  public class CalculateOffersRequestDto
  {
    public List<ProductRequestDto> Products { get; set; } = new();
    public decimal OrderSubtotal { get; set; }
  }

  public class OrderItemRequestDto
  {
    public int ProductId { get; set; }
    public int Quantity { get; set; }
  }

  public class ProductRequestDto
  {
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
  }
}
