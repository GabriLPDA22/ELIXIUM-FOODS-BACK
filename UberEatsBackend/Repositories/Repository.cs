using Microsoft.EntityFrameworkCore;
using UberEatsBackend.Data;

namespace UberEatsBackend.Repositories
{
  public class Repository<T> : IRepository<T> where T : class
  {
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
      _context = context;
      _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
      return await _dbSet.FindAsync(id);
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
      return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
      _dbSet.Add(entity);
      await _context.SaveChangesAsync();
      return entity;
    }

    public virtual async Task<T> AddAsync(T entity)
    {
      _dbSet.Add(entity);
      await _context.SaveChangesAsync();
      return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
      _dbSet.Update(entity);
      await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
      var entity = await GetByIdAsync(id);
      if (entity != null)
      {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
      }
    }
  }
}
