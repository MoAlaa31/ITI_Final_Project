using ITI_Project.Core.Models.Credit;

namespace ITI_Project.Core.Specifications.CreditSpecs
{
    public class PaymentWithPaginationSpecification : BaseSpecifications<Payment>
    {
        public PaymentWithPaginationSpecification(string userId, PaginationSpecParams specParams)
            : base(p => p.UserId == userId)
        {
            AddOrderByDescending(p => p.CreatedAt);
            ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
        }
    }
}
