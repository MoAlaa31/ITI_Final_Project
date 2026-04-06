using ITI_Project.Api.DTO.Moderation;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.ModerationControllers
{
    public class ReviewController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;

        public ReviewController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
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
                Rating = dto.Rating,
                Message = dto.Message
            };

            await unitOfWork.Repository<Review>().AddAsync(review);
            await unitOfWork.CompleteAsync();

            await UpdateProviderRating(review.ProviderId, review.Rating, 1);

            return Ok(new ReviewDto
            {
                Id = review.Id,
                ProviderId = review.ProviderId,
                ServiceRequestId = review.ServiceRequestId,
                Rating = review.Rating,
                Message = review.Message
            });
        }

        [Authorize]
        [HttpGet("provider-reviews/{providerId:int}")]
        public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetProviderReviews(int providerId)
        {
            var providerExists = await unitOfWork.Repository<Provider>().AnyAsync(p => p.Id == providerId);
            if (!providerExists)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var reviews = await unitOfWork.Repository<Review>()
                .GetManyByConditionAsync(r => r.ProviderId == providerId) ?? new List<Review>();

            var dto = reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                ProviderId = r.ProviderId,
                ServiceRequestId = r.ServiceRequestId,
                Rating = r.Rating,
                Message = r.Message
            }).ToList();

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
                ProviderId = review.ProviderId,
                ServiceRequestId = review.ServiceRequestId,
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
