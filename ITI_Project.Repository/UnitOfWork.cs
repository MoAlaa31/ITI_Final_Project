using ITI_Project.Core;
using ITI_Project.Core.IRepository;
using ITI_Project.Core.Models;
using ITI_Project.Repository.Data;
using System.Collections;

namespace ITI_Project.Repository
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly AppDbContext dbContext;
        private Hashtable repositories;
        public UnitOfWork(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            var key = typeof(TEntity).Name;
            if (!repositories.ContainsKey(key))
            {
                var repository = new GenericRepository<TEntity>(dbContext);
                repositories.Add(key, repository);
            }

            return repositories[key] as IGenericRepository<TEntity>;
        }
    }
}