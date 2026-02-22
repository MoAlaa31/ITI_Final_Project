using AutoMapper;
using ITI_Project.Api.DTO.Users;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Helpers;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.UserControllers
{
    public class ProviderController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ProviderController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPost("BeProvider")]
        public async Task<ActionResult> RequestToBeProvider(ProviderDTO providerFromUserDTO)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var clientExists = await unitOfWork.Repository<Client>().AnyAsync(c => c.Id == clientId);
            if (!clientExists)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Client not found"));

            var alreadyProvider = await unitOfWork.Repository<Provider>().AnyAsync(p => p.ClientId == clientId);
            if (alreadyProvider)
                return Conflict(new ApiResponse(StatusCodes.Status409Conflict, "You already have a provider request."));

            var governorateExists = await unitOfWork.Repository<Governorate>().AnyAsync(g => g.Id == providerFromUserDTO.GovernorateId);
            var regionExists = await unitOfWork.Repository<Region>().AnyAsync(r => r.Id == providerFromUserDTO.RegionId);
            if (!governorateExists || !regionExists)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid Governorate or Region"));

            var provider = new Provider
            {
                ClientId = clientId,
                StartedAt = DateHelper.GetTodayInEgypt(),
                VerificationStatus = VerificationStatus.Pending,
                Isverified = false,
                Rating = null,
                JobsCount = 0,
                Nickname = providerFromUserDTO.Nickname,
                Bio = providerFromUserDTO.Bio,
                GovernorateId = providerFromUserDTO.GovernorateId,
                RegionId = providerFromUserDTO.RegionId,
            };

            try
            {
                await unitOfWork.Repository<Provider>().AddAsync(provider);
                await unitOfWork.CompleteAsync();
                return Ok(new ApiResponse(StatusCodes.Status201Created, "Your request to become a provider has been submitted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"An error occurred while processing the request: {ex.Message}"));
            }
        }
    }
}
