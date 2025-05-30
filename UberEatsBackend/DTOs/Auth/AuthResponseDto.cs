// UberEatsBackend/DTOs/Auth/AuthResponseDto.cs - ACTUALIZADO
namespace UberEatsBackend.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? BusinessId { get; set; }

        // NUEVOS CAMPOS PARA FOTO DE PERFIL DE GOOGLE
        public string? PhotoURL { get; set; }
        public string? GoogleId { get; set; }
    }
}
