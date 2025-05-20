// UberEatsBackend/Repositories/IBusinessRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
  public interface IBusinessRepository : IRepository<Business>
  {
    Task<Business?> GetWithDetailsAsync(int id);
    Task<List<Business>> GetByOwnerIdAsync(int userId);
    Task<List<Business>> GetActiveBusinessesAsync();
    Task<bool> IsOwner(int businessId, int userId);
  }
}
