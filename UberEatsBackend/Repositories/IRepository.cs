using System.Collections.Generic;
using System.Threading.Tasks;

namespace UberEatsBackend.Repositories
{
  public interface IRepository<T> where T : class
  {
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
  }
}
