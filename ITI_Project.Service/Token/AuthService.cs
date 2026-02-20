using ITI_Project.Core;
using ITI_Project.Core.DTOs;
using ITI_Project.Core.IServices;
using ITI_Project.Core.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Text;

namespace ITI_Project.Service.Token
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration configuration;
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<AppUser> userManager;

        public AuthService(IConfiguration configuration,
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager)
        {
            this.configuration = configuration;
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }
        public async Task<string> CreateTokenAsync(AppUser user, UserManager<AppUser> userManager)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.GivenName , user.UserName),
                new Claim(ClaimTypes.Email , user.Email),
                new Claim(ClaimTypes.NameIdentifier , user.Id)
            };

            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]));
            var token = new JwtSecurityToken(
                // Payload
                // 1. Registered claims
                audience: configuration["JWT:ValidAudience"],
                issuer: configuration["JWT:ValidIssuer"],
                expires: DateTime.UtcNow.AddMinutes(double.Parse(configuration["JWT:AccessTokenExpirationInMinutes"])),
                // 2. Private claims
                claims: authClaims,
                // 3. secret key
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256Signature)
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RefreshTokenResultDto> RefreshTokenAsync(string refreshToken)
        {
            var user = await userManager.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken));
            // (Any ==> will return true if user has any refresh token that matches the token passed)

            if (user == null)
                return new RefreshTokenResultDto { IsAuthenticated = false, Message = "Invalid refresh token" };

            var DBrefreshToken = user.RefreshTokens.Single(rt => rt.Token == refreshToken);

            if (!DBrefreshToken.IsActive)
                return new RefreshTokenResultDto { IsAuthenticated = false, Message = "Inactive refresh token" };

            DBrefreshToken.RevokedOn = DateTime.UtcNow;
            var newRefreshToken = TokenHelper.GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await userManager.UpdateAsync(user);

            var accessToken = await CreateTokenAsync(user, userManager);

            var roles = await userManager.GetRolesAsync(user);

            return new RefreshTokenResultDto
            {
                IsAuthenticated = true,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpiration = newRefreshToken.ExpiresOn,
            };
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var user = await userManager.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == token));

            if (user == null)
                return false;

            var refreshToken = user.RefreshTokens.Single(rt => rt.Token == token);

            if (!refreshToken.IsActive)
                return false;

            // Revoke the old refresh token and save the changes to the database
            refreshToken.RevokedOn = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            return true;
        }
    }
}
