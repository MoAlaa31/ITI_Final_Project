using AutoMapper;
using ITI_Project.Api.DTO.Requests;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Helpers;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.RequestControllers
{
    public class RequestOfferController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public RequestOfferController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpPost("create-offer/{serviceRequestId:int}")]
        public async Task<ActionResult<RequestOfferDTO>> CreateRequestOffer(
            int serviceRequestId,
            [FromBody] RequestOfferFromUserDTO requestOfferFromUser)
        {
            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out int providerId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

            var providerExists = await unitOfWork.Repository<Provider>().AnyAsync(p => p.Id == providerId);
            if (!providerExists)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Provider not found"));

            var serviceRequest = await unitOfWork.Repository<ServiceRequest>()
                .GetByIdWithIncludesAsync(serviceRequestId, sr => sr.RequestOffers!);

            if (serviceRequest is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Service request not found"));

            if (serviceRequest.RequestStatus != RequestStatus.Open || serviceRequest.ProviderId != null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Cannot create an offer for this request"));

            if (serviceRequest.RequestOffers?.Any(o => o.ProviderId == providerId) == true)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "You have already created an offer for this service request"));

            var requestOffer = mapper.Map<RequestOffer>(requestOfferFromUser);
            requestOffer.CreatedAt = DateHelper.GetNowInEgypt();
            requestOffer.ProviderId = providerId;
            requestOffer.ServiceRequestId = serviceRequestId;

            try
            {
                await unitOfWork.Repository<RequestOffer>().AddAsync(requestOffer);
                await unitOfWork.CompleteAsync();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while creating the request offer"));
            }

            return Ok(mapper.Map<RequestOfferDTO>(requestOffer));
        }

        [Authorize(Roles = $"{nameof(UserRoleType.Provider)},{nameof(UserRoleType.Client)}")]
        [HttpGet("{serviceRequestId:int}")]
        public async Task<ActionResult<RequestOfferDTO>> GetRequestOffersByServiceRequestId(int serviceRequestId)
        {
            var serviceRequest = await unitOfWork.Repository<ServiceRequest>()
                .GetByIdWithIncludesAsync(serviceRequestId, sr => sr.RequestOffers!);

            if (serviceRequest is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Service request not found"));

            var offers = serviceRequest.RequestOffers ?? new List<RequestOffer>();

            var isClient = User.IsInRole(nameof(UserRoleType.Client));
            var isProvider = User.IsInRole(nameof(UserRoleType.Provider));

            if (isClient)
            {
                var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
                if (!int.TryParse(clientIdClaim, out var clientId))
                    return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

                if (serviceRequest.ClientId == clientId)
                {
                    return Ok(mapper.Map<IReadOnlyList<RequestOfferDTO>>(offers));
                }
            }

            if (isProvider)
            {
                var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
                if (!int.TryParse(providerIdClaim, out var providerId))
                    return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

                var providerOffer = offers.FirstOrDefault(o => o.ProviderId == providerId);
                if (providerOffer is null)
                    return Ok(new List<RequestOfferDTO>());

                return Ok(mapper.Map<RequestOfferDTO>(providerOffer));
            }

            return Forbid();
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpPut("update-offer/{serviceRequestId:int}")]
        public async Task<ActionResult<RequestOfferDTO>> UpdateRequestOffer(
            int serviceRequestId,
            [FromBody] RequestOfferFromUserDTO requestOfferFromUser)
        {
            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out int providerId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

            var serviceRequest = await unitOfWork.Repository<ServiceRequest>()
                .GetByIdWithIncludesAsync(serviceRequestId, sr => sr.RequestOffers!);

            if (serviceRequest is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Service request not found"));

            if (serviceRequest.RequestStatus != RequestStatus.Open || serviceRequest.ProviderId != null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Cannot update offer for this request"));

            var offer = serviceRequest.RequestOffers?
                .FirstOrDefault(o => o.ProviderId == providerId);

            if (offer is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Offer not found"));

            offer.Price = requestOfferFromUser.Price;
            offer.Message = requestOfferFromUser.Message;

            unitOfWork.Repository<RequestOffer>().Update(offer);
            await unitOfWork.CompleteAsync();

            return Ok(mapper.Map<RequestOfferDTO>(offer));
        }
    }
}
