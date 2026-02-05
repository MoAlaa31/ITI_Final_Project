using ITI_Project.Core.Models.Identity;
using ITI_Project.Repository.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Repository.Identity
{
    public class AppIdentityDBContext : IdentityDbContext<AppUser>
    {
        public AppIdentityDBContext(DbContextOptions<AppIdentityDBContext> options) : base(options)
        {
            
        }
    }
}
