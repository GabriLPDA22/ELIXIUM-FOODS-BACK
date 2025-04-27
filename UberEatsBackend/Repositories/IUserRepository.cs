using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetWithAddressesAsync(int id);
        Task<List<User>> GetByRoleAsync(string role);
        Task<bool> IsEmailUniqueAsync(string email);
    }
}
