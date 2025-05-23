using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public async Task<List<T>> GetAllAsync()
    {
      return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
      return await _dbSet.FindAsync(id);
    }

    public async Task<T> AddAsync(T entity)
    {
      await _dbSet.AddAsync(entity);
      await _context.SaveChangesAsync();
      return entity;
    }

    public async Task UpdateAsync(T entity)
    {
      _dbSet.Update(entity);
      await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
      _dbSet.Remove(entity);
      await _context.SaveChangesAsync();
    }
  }
}
