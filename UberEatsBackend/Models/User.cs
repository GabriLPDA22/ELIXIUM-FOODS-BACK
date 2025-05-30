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
    public string Role { get; set; } = "Customer"; // Admin, Customer, Restaurant, DeliveryPerson, Business

    public bool IsActive { get; set; } = true;

    public string? RefreshToken { get; set; }  // Token de refresco actual
    public DateTime? RefreshTokenExpiry { get; set; }  // Fecha de expiración

    // NUEVAS PROPIEDADES PARA GOOGLE OAUTH
    public string? GoogleId { get; set; }  // ID único de Google
    public string? PhotoURL { get; set; }  // URL de la foto de perfil (se mantiene igual)

     // Campos para reset de contraseña
    public string? PasswordResetToken { get; set; }  // Token para reset de contraseña
    public DateTime? PasswordResetTokenExpiry { get; set; }  // Fecha de expiración del reset token

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

    // Navigation properties
    public List<Address> Addresses { get; set; } = new List<Address>();
    public List<Order> CustomerOrders { get; set; } = new List<Order>();
    public List<Order> DeliveryOrders { get; set; } = new List<Order>();
    public Business? Business { get; set; }  // Relación con Business

    public User()
    {
      CreatedAt = DateTime.UtcNow;
      UpdatedAt = DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}";
  }
}
