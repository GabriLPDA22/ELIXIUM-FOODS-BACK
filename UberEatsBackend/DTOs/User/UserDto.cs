using System;
using System.Collections.Generic;
using UberEatsBackend.DTOs.Address;

namespace UberEatsBackend.DTOs.User
{
  public class UserDto
  {
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<AddressDto> Addresses { get; set; } = new List<AddressDto>();

    // Additional profile fields
    public string? Birthdate { get; set; }
    public string? Bio { get; set; }
    public List<string>? DietaryPreferences { get; set; }
    public string? PhotoURL { get; set; }
  }
}
