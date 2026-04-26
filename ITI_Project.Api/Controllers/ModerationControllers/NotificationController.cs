using AutoMapper;
using ITI_Project.Api.DTO.Moderation;
using ITI_Project.Api.DTO.Posts;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Helpers;
using ITI_Project.Api.Hubs;
using ITI_Project.Api.Hubs.Interfaces;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Users;
using ITI_Project.Core.Specifications;
using ITI_Project.Core.Specifications.NotificationSpecs;
using ITI_Project.Repository;
using ITI_Project.Repository.Data.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ITI_Project.Api.Controllers.ModerationControllers
{
    public class NotificationController : BaseApiController
    {
        private readonly IHubContext<NotificationHub, INotification> hub;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public NotificationController(IHubContext<NotificationHub, INotification> hub, IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.hub = hub;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPost("send-notification")]
        public async Task<IActionResult> SendNotification(NotificationFromUserDTO notificationFromDb)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);

            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            // Send to the specific client's group
            var notification = new Notification
            {
                Title = "Manual Notification",
                Message = notificationFromDb.Message,
                Type = NotificationType.info,
                CreatedAt = DateHelper.GetNowInEgypt(),
                IsRead = false,
                ClientId = clientId
            };

            await unitOfWork.Repository<Notification>().AddAsync(notification);
            await unitOfWork.CompleteAsync();

            await hub.Clients.Group($"user-{clientId}")
            .ReceiveNotification(new NotificationDTO
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead
            });

            return Ok("Your message sent successfully");
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPut("set-read")]
        public async Task<IActionResult> SetNotificationRead([FromBody] NotificationListDTO notificationIdsDto)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);

            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var notifications = await unitOfWork.Repository<Notification>()
                .GetManyByConditionAsync(n => notificationIdsDto.notificationIds.Contains(n.Id) && n.ClientId == clientId);

            if (notifications == null || notifications.Count == 0)
                return NotFound();

            foreach (var notification in notifications)
                notification.IsRead = true;

            unitOfWork.Repository<Notification>().UpdateRange(notifications);
            await unitOfWork.CompleteAsync();

            return Ok("Notifications set to read successfully");
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] PaginationSpecParams specParams)
        {
            // set max size to 15 to avoid large data transfer
            specParams.SetMaxPageSize(15);

            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var countSpec = new NotificationCountSpecification(clientId);
            var count = await unitOfWork.Repository<Notification>().GetCountAsync(countSpec);

            var spec = new NotificationWithPaginationSpecification(clientId, specParams);
            var notifications = await unitOfWork.Repository<Notification>().GetAllWithSpecAsync(spec);

            var data = mapper.Map<List<NotificationDTO>>(notifications);
            return Ok(new Pagination<NotificationDTO>(specParams.PageIndex, specParams.PageSize, count, data));
        }
    }
}
