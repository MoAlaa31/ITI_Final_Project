using ITI_Project.Core.Models.Moderation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.Specifications.ReportSpecs
{
    public class ReportWithPaginationSpecification : BaseSpecifications<Report>
    {
        public ReportWithPaginationSpecification(PaginationSpecParams specParams)
        {
            AddOrderByDescending(r => r.LastUpdate);
            Includes.Add(r => r.ServiceRequest!);
            ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
        }
    }
}
