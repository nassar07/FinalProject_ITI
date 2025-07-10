using FinalProject_ITI.Models;
using System.Linq.Expressions;

namespace FinalProject_ITI.Repositories.Interfaces;

public interface IBrandRepository<T> where T : class
{
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
}
