using ITI_Project.Core.Models;
using ITI_Project.Core.Models.Persons;
using ITI_Project.Core.Specifications;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ITI_Project.Core.IRepository
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        public Task AddAsync(T entity);
        public Task<T> AddWithSaveAsync(T entity);
        public void Update(T entity);
        public void Delete(T entity);
        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        public Task<IReadOnlyList<T>?> GetAllWithSpecAsync(ISpecifications<T> specs); /* Task<IReadOnlyList<T> */
        public Task<IReadOnlyList<T>?> GetFirstWithSpecAsync(ISpecifications<T> specs, int number);
        public Task<T?> GetWithSpecsAsync(ISpecifications<T> specs);
        public Task<IReadOnlyList<TResult>> GetAllWithSpecAsync<TResult>(ISpecifications<T> spec, Expression<Func<T, TResult>> selector);
        public Task<T?> GetByIdAsync(int id);
        public Task<T?> GetAsync(int id);
        public Task<User> GetByAppUserIdAsync(string id);
        public Task<T?> GetWithNameAsync(string name);
        public Task<IReadOnlyList<T>> GetAllAsync();
        public Task<int> GetCountAsync(ISpecifications<T> spec);
        public Task SaveAsync();
        public void DeleteRange(IEnumerable<T> entities);
        public Task AddRangeAsync(IEnumerable<T> entities);
        public Task<T?> GetByConditionAsync(Expression<Func<T, bool>> expression);
        public Task<IReadOnlyList<T>?> GetManyByConditionAsync(Expression<Func<T, bool>> expression);
    }
}
