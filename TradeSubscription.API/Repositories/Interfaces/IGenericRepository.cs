using System.Linq.Expressions;
using TradeSubscriptionAPI.Models;

namespace TradeSubscriptionAPI.Repositories.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);   // soft delete
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}