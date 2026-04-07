using ITI_Project.Core.Models.Location;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.Specifications.PostSpecs
{
    public class PostSpecParams
    {
        private const int MaxPageSize = 20;
		public int PageIndex { get; set; } = 1;

        private int pageSize;
		public int PageSize
		{
			get { return pageSize; }
            set { pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }

		private string? search;
		public string? Search
		{
			get { return search; }
			set { search = value?.ToLower(); }
		}

		public int? RegionId { get; set; }
		public int? GovernorateId { get; set; }
    }
}
