using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
    public interface IPromotionRepository : IRepository<Promotion>
    {
        Task<List<Promotion>> GetByBusinessIdAsync(int businessId);
        Task<Promotion?> GetByCodeAsync(string code);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
    }
}
