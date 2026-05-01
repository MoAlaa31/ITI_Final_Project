using ITI_Project.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.Specifications.ReportSpecs
{
    public class ReportSpecParams : PaginationSpecParams
    {
        public List<ReportType>? ReportTypes { get; set; }
    }
}
