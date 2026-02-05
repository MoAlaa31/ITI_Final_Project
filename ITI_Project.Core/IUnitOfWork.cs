using ITI_Project.Core.IRepository;
using ITI_Project.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core
{
    public interface IUnitOfWork
    {
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    }
}
