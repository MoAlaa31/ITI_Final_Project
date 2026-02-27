using AutoMapper;
using ITI_Project.Api.DTO.Users;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Filters;
using ITI_Project.Api.Helpers;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Identity;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.UserControllers
{
    public class ProviderController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<AppUser> userManager;

        public ProviderController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
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
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, $"An error occurred while processing the request: {ex.Message}"));
            }
        }

        [Authorize(Roles = nameof(UserRoleType.Admin))]
        [HttpPut("VerifyProvider/{providerId:int}")]
        public async Task<ActionResult> VerifyProvider(int providerId, [FromQuery] bool isVerified)
        {
            // 1- Retrieve the provider and check his data
            var provider = await unitOfWork.Repository<Provider>()
                .GetByIdWithIncludesAsync(
                    providerId,
                    p => p.ProviderServices!,
                    p => p.BaseLocation!,
                    p => p.ProviderDocuments!,
                    p => p.Client);

            if (provider == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var hasBaseLocation = provider.BaseLocation != null;
            var hasServices = provider.ProviderServices != null && provider.ProviderServices.Any();
            var hasDocuments = provider.ProviderDocuments != null && provider.ProviderDocuments.Any();
            var allDocumentsApproved = hasDocuments && provider.ProviderDocuments!.All(d => d.IsApproved);

            if(isVerified && (!hasBaseLocation || !hasServices || !hasDocuments || !allDocumentsApproved))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Provider cannot be verified. Ensure that the provider has a base location, at least one service, and all documents are approved."));

            // 2- Update the provider's verification status if the data is valid
            provider.Isverified = isVerified;
            provider.VerificationStatus = isVerified ? VerificationStatus.Approved : VerificationStatus.Rejected;

            if (isVerified)
            {
                var appUser = await userManager.FindByIdAsync(provider.Client.AppUserId);
                if (appUser != null && !await userManager.IsInRoleAsync(appUser, nameof(UserRoleType.Provider)))
                    await userManager.AddToRoleAsync(appUser, nameof(UserRoleType.Provider));
            }

            unitOfWork.Repository<Provider>().Update(provider);
            await unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(StatusCodes.Status200OK, $"Provider has been {(isVerified ? "verified" : "rejected")} successfully."));
        }
    }
}
