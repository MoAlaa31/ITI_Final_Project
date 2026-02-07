using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.Models.Identity
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
        public List<RefreshToken>? RefreshTokens { get; set; }
        public string? DeviceToken { get; set; }
    }
}
