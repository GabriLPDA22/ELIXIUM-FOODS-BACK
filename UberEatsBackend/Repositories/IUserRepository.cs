using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        Task<User?> GetUserByPasswordResetTokenAsync(string resetToken);
        Task<bool> EmailExistsAsync(string email);
        Task<List<User>> GetUsersByRoleAsync(string role);
        Task<User?> GetUserWithAddressesAsync(int userId);
        Task<User?> GetUserWithBusinessAsync(int userId);
        Task<List<User>> GetActiveUsersAsync();
        Task<int> GetUserCountByRoleAsync(string role);
        Task HardDeleteAsync(int id);
        Task<User> AddAsync(User user);
    }
}
