using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
  public class BusinessRepository : Repository<Business>, IBusinessRepository
  {
    public BusinessRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Business?> GetWithDetailsAsync(int id)
    {
      return await _context.Businesses
          .Include(b => b.User)
          .Include(b => b.Restaurants)
              .ThenInclude(r => r.Address)
          .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Business>> GetActiveBusinessesAsync()
    {
      return await _context.Businesses
          .Include(b => b.User)
          .Include(b => b.Restaurants)
          .Where(b => b.IsActive)
          .OrderBy(b => b.Name)
          .ToListAsync();
    }

    public override async Task<List<Business>> GetAllAsync()
    {
      return await _context.Businesses
          .Include(b => b.User)
          .Include(b => b.Restaurants)
          .OrderBy(b => b.Name)
          .ToListAsync();
    }

    public override async Task<Business?> GetByIdAsync(int id)
    {
      return await GetWithDetailsAsync(id);
    }
  }
}
