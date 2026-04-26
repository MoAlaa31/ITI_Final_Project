using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.Specifications
{
    public class PaginationSpecParams
    {
        public int PageIndex { get; set; } = 1;

        private int pageSize = 10;
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }

        public int MaxPageSize { get; private set; } = 20;
        public void SetMaxPageSize(int max)
        {
            MaxPageSize = max;
        }
    }
}
