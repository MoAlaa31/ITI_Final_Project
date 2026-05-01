using ITI_Project.Core.Enums;

namespace ITI_Project.Api.DTO.Credit
{
    public class PaymentDTO
    {
        public int Id { get; set; }
        public int Credits { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
