using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.DTOs
{
    public class RefreshTokenResultDto
    {
        public bool IsAuthenticated { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }

        public string Message { get; set; }
    }
}
