using AutoMapper;
using ITI_Project.Api.DTO.Moderation;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Helpers;
using ITI_Project.Api.Hubs;
using ITI_Project.Api.Hubs.Interfaces;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.ModerationControllers
{
    public class ReviewController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IHubContext<NotificationHub, INotification> hub;
        private readonly IMapper mapper;

        public ReviewController(IUnitOfWork unitOfWork, IHubContext<NotificationHub, INotification> hub, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.hub = hub;
            this.mapper = mapper;
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPost("create-review")]
        public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] ReviewCreateDto dto)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var clientExists = await unitOfWork.Repository<Client>().AnyAsync(c => c.Id == clientId);
            if (!clientExists)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Client not found"));

            var serviceRequest = await unitOfWork.Repository<ServiceRequest>().GetByIdAsync(dto.ServiceRequestId);
            if (serviceRequest is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Service request not found"));

            if (serviceRequest.ClientId != clientId)
                return Forbid();

            if (!serviceRequest.ProviderId.HasValue)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Service request is not assigned to a provider"));

            var reviewExists = await unitOfWork.Repository<Review>()
                .AnyAsync(r => r.ServiceRequestId == dto.ServiceRequestId);

            if (reviewExists)
                return Conflict(new ApiResponse(StatusCodes.Status409Conflict, "Review already exists for this service request"));

            var review = new Review
            {
                ServiceRequestId = dto.ServiceRequestId,
                ProviderId = serviceRequest.ProviderId.Value,
                ClientId = clientId,
                Rating = dto.Rating,
                Message = dto.Message,
                CreatedAt = DateHelper.GetNowInEgypt()
            };

            var provider = await unitOfWork.Repository<Provider>().GetByIdAsync(serviceRequest.ProviderId.Value);
            if (provider != null)
            {
                // Notify the provider about the review
                var notification = new Notification
                {
                    Title = "تقييم جديد",
                    Message = "تم اضافة تقييم جديد لك",
                    Type = NotificationType.info,
                    CreatedAt = DateHelper.GetNowInEgypt(),
                    IsRead = false,
                    ClientId = provider.ClientId
                };

                await unitOfWork.Repository<Notification>().AddAsync(notification);
                await unitOfWork.CompleteAsync();

                await hub.Clients.Group($"user-{provider.ClientId}")
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
            
            await unitOfWork.Repository<Review>().AddAsync(review);
            await unitOfWork.CompleteAsync();

            await UpdateProviderRating(review.ProviderId, review.Rating, 1);

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Review created successfully"));
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpGet("provider-reviews/{providerId:int}")]
        public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetProviderReviews(int providerId)
        {
            var providerExists = await unitOfWork.Repository<Provider>().AnyAsync(p => p.Id == providerId);
            if (!providerExists)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var reviews = await unitOfWork.Repository<Review>()
                .GetManyByConditionAsync(r => r.ProviderId == providerId, r => r.Client) ?? new List<Review>();

            var dto = mapper.Map<IReadOnlyList<ReviewDto>>(reviews);

            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpGet("my-reviews")]
        public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetMyReviews()
        {
            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out var providerId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

            var providerExists = await unitOfWork.Repository<Provider>().AnyAsync(p => p.Id == providerId);
            if (!providerExists)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var reviews = await unitOfWork.Repository<Review>()
                .GetManyByConditionAsync(r => r.ProviderId == providerId, r => r.Client) ?? new List<Review>();

            var dto = mapper.Map<IReadOnlyList<ReviewDto>>(reviews);

            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPut("update-review/{reviewId:int}")]
        public async Task<ActionResult<ReviewDto>> UpdateReview(int reviewId, [FromBody] ReviewUpdateDto dto)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var review = await unitOfWork.Repository<Review>().GetByIdAsync(reviewId);
            if (review is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Review not found"));

            var serviceRequest = await unitOfWork.Repository<ServiceRequest>().GetByIdAsync(review.ServiceRequestId);
            if (serviceRequest is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Service request not found"));

            if (serviceRequest.ClientId != clientId)
                return Forbid();

            var oldRating = review.Rating;

            review.Rating = dto.Rating;
            review.Message = dto.Message;

            unitOfWork.Repository<Review>().Update(review);
            await unitOfWork.CompleteAsync();

            await UpdateProviderRating(review.ProviderId, review.Rating - oldRating, 0);

            return Ok(new ReviewDto
            {
                Id = review.Id,
                Rating = review.Rating,
                Message = review.Message
            });
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpDelete("delete-review/{reviewId:int}")]
        public async Task<ActionResult> DeleteReview(int reviewId)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var review = await unitOfWork.Repository<Review>().GetByIdAsync(reviewId);
            if (review is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Review not found"));

            var serviceRequest = await unitOfWork.Repository<ServiceRequest>().GetByIdAsync(review.ServiceRequestId);
            if (serviceRequest is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Service request not found"));

            if (serviceRequest.ClientId != clientId)
                return Forbid();

            unitOfWork.Repository<Review>().Delete(review);
            await unitOfWork.CompleteAsync();

            await UpdateProviderRating(review.ProviderId, -review.Rating, -1);

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Review deleted successfully"));
        }

        private async Task UpdateProviderRating(int providerId, double ratingDelta, int countDelta)
        {
            var provider = await unitOfWork.Repository<Provider>().GetByIdAsync(providerId);
            if (provider == null)
                return;

            provider.ReviewsCount = Math.Max(0, provider.ReviewsCount + countDelta);
            provider.RatingSum = provider.ReviewsCount == 0 ? 0 : provider.RatingSum + ratingDelta;
            provider.Rating = provider.ReviewsCount == 0 ? null : provider.RatingSum / provider.ReviewsCount;

            unitOfWork.Repository<Provider>().Update(provider);
            await unitOfWork.CompleteAsync();
        }
    }
}
