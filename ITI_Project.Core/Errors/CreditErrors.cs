using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.Errors
{
    public static class CreditErrors
    {
        public static Error InsufficientCredits =
            new(
                "Credits.Insufficient",
                "Not enough credits",
                HttpStatusCode.BadRequest);
    }
}
