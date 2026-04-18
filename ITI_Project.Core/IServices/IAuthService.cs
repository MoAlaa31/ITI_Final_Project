using ITI_Project.Core.Models.Identity;
using ITI_Project.Core.ServiceDTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.IServices
{
    public interface IAuthService
    {
        Task<string> CreateTokenAsync(AppUser user, UserManager<AppUser> userManager);      // Access Token
        Task<RefreshTokenResultDto> RefreshTokenAsync(string token);                        // Refresh Token
        Task<bool> RevokeTokenAsync(string token);
    }
}
