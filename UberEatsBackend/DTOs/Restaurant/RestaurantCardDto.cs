namespace UberEatsBackend.DTOs.Restaurant
{
  public class RestaurantCardDto
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
    public string? Cuisine { get; set; }
    public int ReviewCount { get; set; }
    public double? Distance { get; set; }
    public bool Featured { get; set; }
    public bool IsNew { get; set; }
    public string? PromoText { get; set; }
    // Agregando los campos para el frontend
    public string? PriceRange { get; set; }
    public int OrderCount { get; set; }
    public int Tipo { get; set; }
  }
}
