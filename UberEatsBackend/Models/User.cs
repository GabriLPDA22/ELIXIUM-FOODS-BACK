using System;
using System.Collections.Generic;

namespace UberEatsBackend.Models
{
  public class User
  {
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer"; // Admin, Customer, Restaurant, DeliveryPerson

    private DateTime _createdAt;
    private DateTime _updatedAt;
    private DateTime? _birthdate;

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

    public DateTime? Birthdate
    {
        get => _birthdate;
        set => _birthdate = value?.Kind == DateTimeKind.Unspecified ?
            DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) :
            value?.ToUniversalTime();
    }

    // Additional profile fields
    public string? Bio { get; set; }
    public string? DietaryPreferencesJson { get; set; } // Stored as JSON string
    public string? PhotoURL { get; set; }

    // Navigation properties
    public List<Address> Addresses { get; set; } = new List<Address>();
    public Restaurant? Restaurant { get; set; }
    public List<Order> CustomerOrders { get; set; } = new List<Order>();
    public List<Order> DeliveryOrders { get; set; } = new List<Order>();

    public User()
    {
      CreatedAt = DateTime.UtcNow;
      UpdatedAt = DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}";
  }
}
