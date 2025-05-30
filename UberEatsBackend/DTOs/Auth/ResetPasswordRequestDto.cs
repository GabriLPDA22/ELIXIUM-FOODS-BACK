
// DTOs/Auth/ResetPasswordRequestDto.cs
namespace UberEatsBackend.DTOs.Auth
{
    public class ResetPasswordRequestDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
