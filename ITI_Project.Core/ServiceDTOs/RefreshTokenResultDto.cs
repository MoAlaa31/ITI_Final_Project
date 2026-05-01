using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.ServiceDTOs
{
    public class RefreshTokenResultDto
    {
        public bool IsAuthenticated { get; set; }

        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public DateTime RefreshTokenExpiration { get; set; }

        public string Message { get; set; } = null!;
    }
}
