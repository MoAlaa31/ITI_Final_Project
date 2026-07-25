using ITI_Project.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.Errors
{
    public record Error(string Code, string Message, HttpStatusCode StatusCode)
    {
        public static readonly Error None = new(string.Empty, string.Empty, HttpStatusCode.OK);
    }
}
