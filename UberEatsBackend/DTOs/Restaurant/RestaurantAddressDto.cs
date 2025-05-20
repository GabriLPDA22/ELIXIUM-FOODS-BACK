// UberEatsBackend/DTOs/Restaurant/RestaurantAddressDto.cs
namespace UberEatsBackend.DTOs.Restaurant
{
  public class RestaurantAddressDto
  {
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Interior { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Name { get; set; } = string.Empty; // Por ejemplo, "Sede Principal"
  }

  // Modificar el CreateRestaurantDto para usar este DTO específico
  public class CreateRestaurantDtoV2
  {
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public decimal DeliveryFee { get; set; }
    public int EstimatedDeliveryTime { get; set; }
    public RestaurantAddressDto Address { get; set; } = null!;
    public int Tipo { get; set; } = 1;
    public int? BusinessId { get; set; }
  }
}
