using ITI_Project.Api.Attributes;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Filters;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Posts;
using ITI_Project.Core.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.PostControllers
{
    public class CommentReactionController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;

        public CommentReactionController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [ServiceFilter(typeof(ExistingIdFilter<Comment>))]
        [HttpPut("react-to-comment/{commentId}")]
        public async Task<ActionResult> AddReactionToComment(int commentId,[FromQuery][ValidEnum<ReactionType>] ReactionType reaction)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var clientExists = await unitOfWork.Repository<Client>().AnyAsync(c => c.Id == clientId);
            if (!clientExists)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Client not found"));

            var reactionFromDb = await unitOfWork.Repository<CommentReaction>()
                .GetByConditionAsync(r => r.CommentId == commentId && r.ClientId == clientId);

            if (reactionFromDb != null)
            {
                if (reactionFromDb.ReactionType == reaction)
                {
                    unitOfWork.Repository<CommentReaction>().Delete(reactionFromDb);
                    await unitOfWork.CompleteAsync();
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Reaction removed successfully"));
                }
                reactionFromDb.ReactionType = reaction;
                unitOfWork.Repository<CommentReaction>().Update(reactionFromDb);
            }
            else
            {
                var newReaction = new CommentReaction
                {
                    CommentId = commentId,
                    ClientId = clientId,
                    ReactionType = reaction
                };
                await unitOfWork.Repository<CommentReaction>().AddAsync(newReaction);
            }
            await unitOfWork.CompleteAsync();
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Reaction updated successfully"));
        }
    }
}
