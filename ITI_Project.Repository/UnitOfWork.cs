using ITI_Project.Core;
using ITI_Project.Core.IRepository;
using ITI_Project.Core.Models;
using ITI_Project.Repository.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections;

namespace ITI_Project.Repository
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly AppDbContext dbContext;
        private readonly Dictionary<Type, object> repositories;
        private IDbContextTransaction? _transaction;
        public UnitOfWork(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
            repositories = new Dictionary<Type, object>();
        }

        public async Task<int> CompleteAsync()
            => await dbContext.SaveChangesAsync();

        public bool HasChanges()
            => dbContext.ChangeTracker.HasChanges();

        public async Task BeginTransactionAsync()
        {
            _transaction = await dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
                await _transaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            var type = typeof(TEntity);

            if (!repositories.ContainsKey(type))
            {
                repositories[type] = new GenericRepository<TEntity>(dbContext);
            }

            return (IGenericRepository<TEntity>)repositories[type];
        }
    }
}