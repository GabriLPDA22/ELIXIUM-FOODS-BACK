using System.Threading.Tasks;
using UberEatsBackend.DTOs.User;
using UberEatsBackend.Models;

namespace UberEatsBackend.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(CreateUserDto userDto);
        Task<User?> UpdateUserAsync(int userId, UpdateUserDto userDto);
        Task<bool> UpdateUserProfileAsync(int userId, UpdateProfileDto profileDto);
        Task<bool> UpdateUserPasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> UpdateUserRoleAsync(int userId, string newRole);
    }
}
