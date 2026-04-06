using ITI_Project.Core.Models.Posts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.Specifications.PostSpecs
{
    public class CountPostsWithFilterSpecification : BaseSpecifications<Post>
    {
        public CountPostsWithFilterSpecification(PostSpecParams specParams) :
            base(p =>
                (string.IsNullOrEmpty(specParams.Search) ||
                    p.Title.ToLower().Contains(specParams.Search.ToLower()) ||
                    (p.Description != null && p.Description.ToLower().Contains(specParams.Search.ToLower()))
                )
                &&
                (specParams.GovernorateId == null || p.GovernorateId == specParams.GovernorateId)
                &&
                (specParams.RegionId == null || (specParams.GovernorateId != null && p.RegionId == specParams.RegionId))
            )
        {
            
        }
    }
}
