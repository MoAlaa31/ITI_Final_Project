using ITI_Project.Core.Models.Posts;
using Microsoft.EntityFrameworkCore;

namespace ITI_Project.Core.Specifications.CommentSpecs
{
    public class CommentsForPostWithPaginationSpecification : BaseSpecifications<Comment>
    {
        public CommentsForPostWithPaginationSpecification(int postId, CommentSpecParams specParams)
            : base(c => c.PostId == postId)
        {
            AddOrderByDescending(c => c.CreatedAt);
            Includes.Add(c => c.Reactions!);
            Includes.Add(c => c.Client);
            ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
        }
    }
}
