using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FinalProject_ITI.Repositories.Implementations;

public class BazarBrandRepository<T> : IBazarBrandRepository<T> where T : class
{
    private readonly AppDbContext _Context;
    public BazarBrandRepository(AppDbContext Context)
    {
        _Context = Context;
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _Context.Set<T>().AnyAsync(predicate);
    }

    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _Context.Set<T>().FirstOrDefaultAsync(predicate);
    }
}
