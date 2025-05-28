using UberEatsBackend.DTOs.User;
using UberEatsBackend.Models;

namespace UberEatsBackend.Services
{
    public interface IUserService
    {
        // Métodos básicos CRUD
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<List<UserDto>> GetUsersByRoleAsync(string role);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task DeleteUserAsync(int id);

        // Métodos de autenticación
        Task<User?> AuthenticateAsync(string email, string password);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetUserEntityByEmailAsync(string email);
        Task<User?> GetUserEntityByIdAsync(int id);

        // Métodos de perfil
        Task UpdateProfileAsync(int userId, UpdateProfileDto updateProfileDto);
        Task<UserDto?> GetUserWithAddressesAsync(int userId);
        Task<UserDto?> GetUserWithBusinessAsync(int userId);

        // Métodos de contraseña
        Task UpdatePasswordAsync(int userId, string newPassword);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task UpdateUserRoleAsync(int userId, string newRole);

        // Métodos de refresh token
        Task SetRefreshTokenAsync(int userId, string refreshToken, DateTime expiry);
        Task ClearRefreshTokenAsync(int userId);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);

        // Métodos de password reset
        Task SetPasswordResetTokenAsync(int userId, string resetToken, DateTime expiry);
        Task ClearPasswordResetTokenAsync(int userId);
        Task<User?> GetUserByPasswordResetTokenAsync(string resetToken);

        // Métodos de estadísticas
        Task<int> GetUserCountByRoleAsync(string role);
        Task<List<UserDto>> GetActiveUsersAsync();

        // Métodos de validación
        bool ValidatePassword(string password, string hash);
        string HashPassword(string password);
    }
}
