using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Repository.Identity
{
    public class RoleSeed
    {
        public static async Task RoleSeedAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Provider" };
            foreach (var role in roles)
            {
                if (! await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
