using AutoMapper;
using ITI_Project.Api.DTO.Posts;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Helpers;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Posts;
using ITI_Project.Core.Models.Users;
using ITI_Project.Core.Specifications.CommentSpecs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.PostControllers
{
    public class CommentController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public CommentController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPost("add-comment")]
        public async Task<ActionResult<CommentCreateResultDTO>> AddComment(CommentCreateDTO dto)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var clientExists = await unitOfWork.Repository<Client>().AnyAsync(c => c.Id == clientId);
            if (!clientExists)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Client not found"));

            var postExists = await unitOfWork.Repository<Post>().AnyAsync(p => p.Id == dto.PostId);
            if (!postExists)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Post not found"));

            var comment = new Comment
            {
                ClientId = clientId,
                PostId = dto.PostId,
                Message = dto.Message,
                CreatedAt = DateHelper.GetNowInEgypt()
            };

            try
            {
                await unitOfWork.Repository<Comment>().AddAsync(comment);
                await unitOfWork.CompleteAsync();

                var result = mapper.Map<CommentCreateResultDTO>(comment);
                return Ok(result);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while creating the comment"));
            }
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpDelete("delete-comment/{commentId:int}")]
        public async Task<ActionResult> DeleteComment(int commentId)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var comment = await unitOfWork.Repository<Comment>().GetByIdAsync(commentId);
            if (comment == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Comment not found"));

            if (comment.ClientId != clientId)
                return Forbid();

            try
            {
                unitOfWork.Repository<Comment>().Delete(comment);
                await unitOfWork.CompleteAsync();

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Comment deleted successfully"));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while deleting the comment"));
            }
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpGet("get-post-comments/{postId:int}")]
        public async Task<ActionResult<IReadOnlyList<CommentDTO>>> GetPostComments(int postId, [FromQuery] CommentSpecParams specParams)
        {
            var postExists = await unitOfWork.Repository<Post>().AnyAsync(p => p.Id == postId);
            if (!postExists)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Post not found"));

            var comments = await unitOfWork.Repository<Comment>()
                .GetAllWithSpecAsync(new CommentsForPostWithPaginationSpecification(postId, specParams)) ?? new List<Comment>();

            var count = await unitOfWork.Repository<Comment>()
                .GetCountAsync(new CountCommentsForPostSpecification(postId));

            var data = mapper.Map<List<CommentDTO>>(comments);

            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (int.TryParse(clientIdClaim, out var clientId))
            {
                var commentIds = data.Select(c => c.Id).ToList();
                var reactions = await unitOfWork.Repository<CommentReaction>()
                    .GetManyByConditionAsync(r => r.ClientId == clientId && commentIds.Contains(r.CommentId)) ?? new List<CommentReaction>();

                var reactionByCommentId = reactions.ToDictionary(r => r.CommentId, r => r.ReactionType);

                foreach (var comment in data)
                    comment.UserReaction = reactionByCommentId.TryGetValue(comment.Id, out var reaction) ? reaction : null;
            }

            return Ok(new Pagination<CommentDTO>(specParams.PageIndex, specParams.PageSize, count, data));
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPut("update-comment/{commentId:int}")]
        public async Task<ActionResult> UpdateComment(int commentId, [FromBody] CommentUpdateDTO dto)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var comment = await unitOfWork.Repository<Comment>().GetByIdAsync(commentId);
            if (comment == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Comment not found"));

            if (comment.ClientId != clientId)
                return Forbid();

            comment.Message = dto.Message;

            unitOfWork.Repository<Comment>().Update(comment);
            await unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Comment updated successfully"));
        }
    }
}
