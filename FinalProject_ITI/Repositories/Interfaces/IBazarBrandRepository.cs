using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FinalProject_ITI.Repositories.Interfaces
{
    public interface IBazarBrandRepository<T> where T : class
    {
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    }
}
