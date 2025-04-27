using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetWithAddressesAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetByRoleAsync(string role)
        {
            return await _context.Users
                .Where(u => u.Role == role)
                .ToListAsync();
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}
