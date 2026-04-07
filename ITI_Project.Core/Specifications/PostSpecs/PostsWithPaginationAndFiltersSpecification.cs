using ITI_Project.Core.Models.Posts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.Specifications.PostSpecs
{
    public class PostsWithPaginationAndFiltersSpecification : BaseSpecifications<Post>
    {
        public PostsWithPaginationAndFiltersSpecification(PostSpecParams specParams) :
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
            AddOrderByDescending(p => p.CreatedAt);

            Includes.Add(p => p.Comments!);
            Includes.Add(p => p.PostImages!);
            Includes.Add(p => p.Reactions!);
            Includes.Add(p => p.Client);
            Includes.Add(p => p.Client.Provider!);

            ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
        }
    }
}
