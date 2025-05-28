// UberEatsBackend/DTOs/Auth/GoogleUserDto.cs
namespace UberEatsBackend.DTOs.Auth
{
  public class GoogleUserDto
  {
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string GoogleId { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
  }
}
