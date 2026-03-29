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
using ITI_Project.Core.Models.Services;
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
                return StatusCode(StatusCodes.Status201Created, new ApiResponse(StatusCodes.Status201Created, "Your request to become a provider has been submitted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, $"An error occurred while processing the request: {ex.Message}"));
            }
        }

        //[Authorize(Roles = nameof(UserRoleType.Client))]
        //[HttpPatch("update-provider-info")]
        //public async Task<ActionResult<ProviderDTO>> UpdateProviderInfo(ProviderFromUserDTO providerFromUserDTO)
        //{
        //    var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
        //    if(!int.TryParse(clientIdClaim, out var clientId))
        //        return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

        //    var providerFromDb = await unitOfWork.Repository<Provider>().GetByConditionAsync(p => p.ClientId == clientId);
        //    if(providerFromDb == null)
        //        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

        //    var governorateExists = await unitOfWork.Repository<Governorate>().AnyAsync(g => g.Id == providerFromUserDTO.GovernorateId);
        //    var regionExists = await unitOfWork.Repository<Region>().AnyAsync(r => r.Id == providerFromUserDTO.RegionId);
        //    if (!governorateExists || !regionExists)
        //        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid Governorate or Region"));

        //    providerFromDb.Nickname = providerFromUserDTO.Nickname;
        //    providerFromDb.Bio = providerFromUserDTO.Bio;
        //    providerFromDb.GovernorateId = providerFromUserDTO.GovernorateId;
        //    providerFromDb.RegionId = providerFromUserDTO.RegionId;

        //    try
        //    {
        //        unitOfWork.Repository<Provider>().Update(providerFromDb);
        //        await unitOfWork.CompleteAsync();

        //        var dto = mapper.Map<ProviderDTO>(providerFromDb);
        //        return Ok(dto);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, $"An error occurred while processing the request: {ex.Message}"));
        //    }
        //}

        [Authorize(Roles = nameof(UserRoleType.Admin))]
        [HttpPatch("VerifyProvider/{providerId:int}")]
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

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPatch("update-provider-profile")]
        public async Task<ActionResult<ProviderDTO>> UpdateProviderProfile(ProviderProfileUpdateDTO dto)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var providerFromDb = await unitOfWork.Repository<Provider>().GetByConditionAsync(p => p.ClientId == clientId);
            if (providerFromDb == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var governorateExists = await unitOfWork.Repository<Governorate>().AnyAsync(g => g.Id == dto.GovernorateId);
            var regionExists = await unitOfWork.Repository<Region>().AnyAsync(r => r.Id == dto.RegionId);
            if (!governorateExists || !regionExists)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid Governorate or Region"));

            providerFromDb.Nickname = dto.Nickname;
            providerFromDb.Bio = dto.Bio;
            providerFromDb.GovernorateId = dto.GovernorateId;
            providerFromDb.RegionId = dto.RegionId;

            if (dto.BaseLocation != null)
            {
                var baseLocation = await unitOfWork.Repository<BaseLocation>()
                    .GetByConditionAsync(bl => bl.ProviderId == providerFromDb.Id);

                if (baseLocation != null)
                {
                    mapper.Map(dto.BaseLocation, baseLocation);
                    unitOfWork.Repository<BaseLocation>().Update(baseLocation);
                }
                else
                {
                    baseLocation = mapper.Map<BaseLocation>(dto.BaseLocation);
                    baseLocation.ProviderId = providerFromDb.Id;
                    await unitOfWork.Repository<BaseLocation>().AddAsync(baseLocation);
                }
            }

            var distinctIds = dto.ServiceIds.Distinct().ToList();
            if (distinctIds.Count > 2)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "A provider can only offer up to 2 services"));

            if (distinctIds.Count > 0)
            {
                var services = await unitOfWork.Repository<Service>().GetManyByConditionAsync(s => distinctIds.Contains(s.Id)) ?? new List<Service>();
                if (services.Count != distinctIds.Count)
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "One or more ServiceIds are invalid"));

                // approach: get existing services, compare with new list, add new ones and remove unchecked ones
                var existing = await unitOfWork.Repository<ProviderService>()
                    .GetManyByConditionAsync(ps => ps.ProviderId == providerFromDb.Id) ?? new List<ProviderService>();

                var existingIds = existing.Select(x => x.ServiceId).ToHashSet();
                var newIds = distinctIds.ToHashSet();

                var toRemove = existing.Where(ps => !newIds.Contains(ps.ServiceId)).ToList();
                var toAdd = newIds.Where(id => !existingIds.Contains(id))
                    .Select(id => new ProviderService { ProviderId = providerFromDb.Id, ServiceId = id })
                    .ToList();

                if (toRemove.Count > 0)
                    unitOfWork.Repository<ProviderService>().DeleteRange(toRemove);

                if (toAdd.Count > 0)
                    await unitOfWork.Repository<ProviderService>().AddRangeAsync(toAdd);
            }

            try
            {
                unitOfWork.Repository<Provider>().Update(providerFromDb);
                await unitOfWork.CompleteAsync();

                var response = mapper.Map<ProviderDTO>(providerFromDb);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, $"An error occurred while processing the request: {ex.Message}"));
            }
        }
    }
}
