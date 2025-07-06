using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI.Repositories.Implementations;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }
    public async Task<IEnumerable<T>> GetAll() => await _dbSet.ToListAsync();
    public async Task<T?> GetById(int id) => await _dbSet.FindAsync(id);
    public async Task Add(T entity) => await _dbSet.AddAsync(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Delete(T entity) => _dbSet.Remove(entity);
    public async Task SaveChanges() => await _context.SaveChangesAsync();
}

