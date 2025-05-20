using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
  public interface IBusinessRepository : IRepository<Business>
  {
    Task<Business?> GetWithDetailsAsync(int id);
    Task<List<Business>> GetActiveBusinessesAsync();
  }
}
