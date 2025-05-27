// UberEatsBackend/Repositories/PromotionRepository.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UberEatsBackend.Data;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
    public class PromotionRepository : Repository<Promotion>, IPromotionRepository
    {
        public PromotionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Promotion>> GetByBusinessIdAsync(int businessId)
        {
            return await _context.Set<Promotion>()
                .Where(p => p.BusinessId == businessId)
                .ToListAsync();
        }

        public async Task<Promotion?> GetByCodeAsync(string code)
        {
            return await _context.Set<Promotion>()
                .FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return !await _context.Set<Promotion>()
                    .AnyAsync(p => p.Code == code && p.Id != excludeId.Value);
            }
            else
            {
                return !await _context.Set<Promotion>()
                    .AnyAsync(p => p.Code == code);
            }
        }
    }
}
