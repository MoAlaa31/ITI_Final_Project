using ITI_Project.Core.Enums;

namespace ITI_Project.Api.DTO.Credit
{
    public class CreditTransactionDTO
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public TransactionType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReferenceId { get; set; } = null!;
    }
}
