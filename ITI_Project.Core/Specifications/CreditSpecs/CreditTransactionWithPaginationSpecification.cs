using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITI_Project.Core.Models.Credit;

namespace ITI_Project.Core.Specifications.CreditSpecs
{
    public class CreditTransactionWithPaginationSpecification : BaseSpecifications<CreditTransaction>
    {
        public CreditTransactionWithPaginationSpecification(int providerId, PaginationSpecParams specParams)
            : base(t => t.ProviderId == providerId)
        {
            AddOrderByDescending(t => t.CreatedAt);
            ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
        }
    }
}
