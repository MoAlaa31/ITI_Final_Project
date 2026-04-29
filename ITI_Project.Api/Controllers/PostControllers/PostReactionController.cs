using ITI_Project.Api.Attributes;
using ITI_Project.Api.DTO.Moderation;
using ITI_Project.Api.DTO.Posts;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Filters;
using ITI_Project.Api.Helpers;
using ITI_Project.Api.Hubs;
using ITI_Project.Api.Hubs.Interfaces;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Posts;
using ITI_Project.Core.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.PostControllers
{
    public class PostReactionController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IHubContext<NotificationHub, INotification> hub;

        public PostReactionController(IUnitOfWork unitOfWork, IHubContext<NotificationHub, INotification> hub)
        {
            this.unitOfWork = unitOfWork;
            this.hub = hub;
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        //[ServiceFilter(typeof(ExistingIdFilter<Post>))]
        [HttpPut("react-to-post/{postId:int}")]
        public async Task<ActionResult> AddReactionToPost(int postId, [FromQuery][ValidEnum<ReactionType>] ReactionType reaction)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var clientExists = await unitOfWork.Repository<Client>().AnyAsync(c => c.Id == clientId);
            if (!clientExists)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Client not found"));

            var post = await unitOfWork.Repository<Post>().GetByIdAsync(postId);
            if (post == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Post not found"));

            var reactionFromDb = await unitOfWork.Repository<PostReaction>()
                .GetByConditionAsync(r => r.ServicePostId == postId && r.ClientId == clientId);

            if (reactionFromDb != null)
            {
                if (reactionFromDb.ReactionType == reaction)
                {
                    unitOfWork.Repository<PostReaction>().Delete(reactionFromDb);
                    await unitOfWork.CompleteAsync();
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Reaction removed successfully"));
                }
                reactionFromDb.ReactionType = reaction;
                unitOfWork.Repository<PostReaction>().Update(reactionFromDb);
            }
            else
            {
                var newReaction = new PostReaction
                {
                    ServicePostId = postId,
                    ClientId = clientId,
                    ReactionType = reaction
                };

                await unitOfWork.Repository<PostReaction>().AddAsync(newReaction);

                // Send notification to the post owner if the reactor is not the post owner and owner is not null
                if (post.ClientId != null && post.ClientId != clientId) 
                {
                    var fiveMinutesAgo = DateHelper.GetNowInEgypt().AddMinutes(-5);
                    var recentNotificationExists = await unitOfWork.Repository<Notification>()
                        .AnyAsync(n =>
                            n.ClientId == post.ClientId.Value &&
                            n.Type == NotificationType.success &&
                            n.Title == "تم اضافة اعجاب" &&
                            n.CreatedAt >= fiveMinutesAgo);

                    if (!recentNotificationExists)
                    {
                        var notification = new Notification
                        {
                            Title = "تم اضافة اعجاب",
                            Message = "تم اضافة اعجاب لك من احدهم",
                            Type = NotificationType.success,
                            CreatedAt = DateHelper.GetNowInEgypt(),
                            IsRead = false,
                            ClientId = post.ClientId.Value
                        };

                        await unitOfWork.Repository<Notification>().AddAsync(notification);

                        await hub.Clients.Group($"user-{post.ClientId.Value}")
                            .ReceiveNotification(new NotificationDTO
                            {
                                Id = notification.Id,
                                Title = notification.Title,
                                Message = notification.Message,
                                Type = notification.Type,
                                CreatedAt = notification.CreatedAt,
                                IsRead = notification.IsRead
                            });
                    }
                }
            }

            await unitOfWork.CompleteAsync();
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Reaction updated successfully"));
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [ServiceFilter(typeof(ExistingIdFilter<Post>))]
        [HttpGet("post-reactions/{postId:int}")]
        public async Task<ActionResult<IReadOnlyList<PostReactionDetailsDTO>>> GetPostReactions(int postId)
        {
            var reactions = await unitOfWork.Repository<PostReaction>()
                .GetManyByConditionAsync(r => r.ServicePostId == postId, r => r.Client!) ?? new List<PostReaction>();

            var data = reactions.Select(r => new PostReactionDetailsDTO
            {
                ClientId = r.ClientId,
                ClientName = $"{r.Client?.FirstName} {r.Client?.LastName}".Trim(),
                ClientPictureUrl = r.Client?.PictureUrl,
                ReactionType = r.ReactionType
            }).ToList();

            return Ok(data);
        }
    }
}
