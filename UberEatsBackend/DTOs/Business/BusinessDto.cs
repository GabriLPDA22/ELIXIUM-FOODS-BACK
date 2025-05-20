// UberEatsBackend/DTOs/Business/BusinessDto.cs
using System;
using System.Collections.Generic;
using UberEatsBackend.DTOs.Restaurant;

namespace UberEatsBackend.DTOs.Business
{
  public class BusinessDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string BusinessType { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<RestaurantDto> Restaurants { get; set; } = new List<RestaurantDto>();
    public string CoverImageUrl { get; set; } = string.Empty;
  }
}
