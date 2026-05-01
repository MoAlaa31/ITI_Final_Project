using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Moderation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.Specifications.ReportSpecs
{
    public class ReportCountSpecification : BaseSpecifications<Report>
    {
        public ReportCountSpecification(ReportSpecParams specParams)
            : base(r =>
                r.Status == ReportStatus.UnderReview &&
                (specParams.ReportTypes == null || specParams.ReportTypes.Count == 0 || specParams.ReportTypes.Contains(r.ReportType)))
        {

        }
    }
}
