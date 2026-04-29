using ITI_Project.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.Models.Credit
{
    public class CreditTransaction : BaseEntity
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public int Amount { get; set; }
        public TransactionType Type { get; set; }
        public String ReferenceId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
