using ITI_Project.Core.Models;
using ITI_Project.Core.Models.Users;
using ITI_Project.Core.Specifications;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ITI_Project.Core.IRepository
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task AddAsync(T entity);
        Task<T> AddWithSaveAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<IReadOnlyList<T>?> GetAllWithSpecAsync(ISpecifications<T> specs);
        Task<IReadOnlyList<T>?> GetFirstWithSpecAsync(ISpecifications<T> specs, int number);
        Task<T?> GetWithSpecsAsync(ISpecifications<T> specs);

        Task<IReadOnlyList<TResult>> GetAllWithSpecAsync<TResult>(ISpecifications<T> spec, Expression<Func<T, TResult>> selector);

        Task<T?> GetByIdAsync(int id);
        Task<T?> GetAsync(int id);
        Task<Client?> GetByAppUserIdAsync(string id);
        Task<T?> GetWithNameAsync(string name);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<int> GetCountAsync(ISpecifications<T> spec);
        Task SaveAsync();
        void DeleteRange(IEnumerable<T> entities);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task<T?> GetByConditionAsync(Expression<Func<T, bool>> predicate);
        Task<IReadOnlyList<T>?> GetManyByConditionAsync(Expression<Func<T, bool>> predicate);
        Task<IReadOnlyList<T>?> GetManyByConditionAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);
        Task<T?> GetByIdWithIncludesAsync(int id, params Expression<Func<T, object>>[] includes);
    }
}
