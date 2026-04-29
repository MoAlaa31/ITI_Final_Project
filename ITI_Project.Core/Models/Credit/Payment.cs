using ITI_Project.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.Models.Credit
{
    public class Payment : BaseEntity
    {
        public int Id { get; set; }
        public string StripePaymentIntentId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public int Credits { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
