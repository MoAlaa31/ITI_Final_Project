using ITI_Project.Core.IRepository;
using ITI_Project.Core.Models;
using ITI_Project.Core.Models.Persons;
using ITI_Project.Core.Specifications;
using ITI_Project.Repository.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ITI_Project.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly AppDbContext dbContext;

        public GenericRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddAsync(T entity)
        {
            await dbContext.Set<T>().AddAsync(entity);
        }

        public async Task<T> AddWithSaveAsync(T entity)
        {
            await dbContext.Set<T>().AddAsync(entity);
            await dbContext.SaveChangesAsync(); // Save changes immediately to get the ID

            return entity;
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await dbContext.Set<T>().AnyAsync(predicate);
        }

        public async Task<T?> GetAsync(int id)
        {
            return await dbContext.Set<T>().FindAsync(id);
        }

        public void Delete(T entity)
        {
            dbContext.Set<T>().Remove(entity);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await dbContext.Set<T>().ToListAsync();
        }

        public async Task<IReadOnlyList<T>?> GetAllWithSpecAsync(ISpecifications<T> specs)
        {
            return (IReadOnlyList<T>?)await ApplyQuery(specs).ToListAsync();
        }

        public Task<IReadOnlyList<TResult>> GetAllWithSpecAsync<TResult>(ISpecifications<T> spec, Expression<Func<T, TResult>> selector)
        {
            throw new NotImplementedException();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await dbContext.Set<T>().FindAsync(id);
        }

        public async Task<int> GetCountAsync(ISpecifications<T> spec)
        {
            return await ApplyQuery(spec).CountAsync();
        }

        public Task<IReadOnlyList<T>?> GetFirstWithSpecAsync(ISpecifications<T> specs, int number)
        {
            throw new NotImplementedException();
        }

        public Task<T?> GetWithNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<T?> GetWithSpecsAsync(ISpecifications<T> specs)
        {
            return await ApplyQuery(specs).FirstOrDefaultAsync();
        }

        public void Update(T entity)
        {
            dbContext.Set<T>().Update(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await dbContext.Set<T>().AddRangeAsync(entities);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            dbContext.Set<T>().RemoveRange(entities);
        }

        public IQueryable<T> ApplyQuery(ISpecifications<T> specs) //Helper Method
        {
            return SpecificationsEvaluator<T>.GetQuery(dbContext.Set<T>(), specs);
        }

        public async Task<T?> GetByConditionAsync(Expression<Func<T, bool>> expression)
        {
            return await dbContext.Set<T>().FirstOrDefaultAsync(expression);
        }

        public async Task SaveAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<T>?> GetManyByConditionAsync(Expression<Func<T, bool>> expression)
        {
            return await dbContext.Set<T>().Where(expression).ToListAsync();

        }

        public async Task<User?> GetByAppUserIdAsync(string appUserId)
        {
            return await dbContext.Set<User>().FirstOrDefaultAsync(u => u.AppUserId == appUserId);
        }
    }
}
