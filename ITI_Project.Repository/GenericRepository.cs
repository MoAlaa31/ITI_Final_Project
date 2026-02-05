using ITI_Project.Core.IRepository;
using ITI_Project.Core.Models;
using ITI_Project.Repository.Data;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
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
    }
}
