using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,        // Created but not confirmed yet
        Processing = 1,     // Payment is being processed (Stripe async)
        Completed = 2,      // Payment succeeded
        Failed = 3,         // Payment failed
        Cancelled = 4,      // User cancelled payment
        Refunded = 5        // Money returned to user
    }
}
