using ITI_Project.Core.Models.Identity;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ITI_Project.Services.Token
{
    public static class TokenHelper
    {
        public static RefreshToken GenerateRefreshToken()
        {
            //var randomNumber = new byte[32];

            //using var generator = new RNGCryptoServiceProvider();
            //generator.GetBytes(randomNumber);

            var randomNumber = RandomNumberGenerator.GetBytes(32);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(30),
                CreatedOn = DateTime.UtcNow
            };
        }
    }
}
