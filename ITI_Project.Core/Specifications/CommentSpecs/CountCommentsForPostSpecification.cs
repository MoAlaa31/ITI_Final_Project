using ITI_Project.Core.Models.Posts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.Specifications.CommentSpecs
{
    public class CountCommentsForPostSpecification : BaseSpecifications<Comment>
    {
        public CountCommentsForPostSpecification(int postId)
            : base(c => c.PostId == postId)
        {
            
        }
    }
}
