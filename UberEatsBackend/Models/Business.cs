using System;
using System.Collections.Generic;

namespace UberEatsBackend.Models
{
  public class Business
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string BusinessType { get; set; } = "Restaurant";

    public int? UserId { get; set; }

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

    public virtual User? User { get; set; }
    public List<Restaurant> Restaurants { get; set; } = new List<Restaurant>();

    public Business()
    {
      CreatedAt = DateTime.UtcNow;
      UpdatedAt = DateTime.UtcNow;
    }
  }
}
