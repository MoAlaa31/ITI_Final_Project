using AutoMapper;
using ITI_Project.Api.DTO.Services;
using ITI_Project.Api.DTO.Users;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Filters;
using ITI_Project.Api.Helpers;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Identity;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Services;
using ITI_Project.Core.Models.Users;
using ITI_Project.Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;
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

        #region Request to be provider
        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPost("BeProvider")]
        public async Task<ActionResult> RequestToBeProvider(ProviderDTO providerFromUserDTO)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var client = await unitOfWork.Repository<Client>().GetByIdAsync(clientId);
            if (client == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Client not found"));

            var alreadyProvider = await unitOfWork.Repository<Provider>().AnyAsync(p => p.ClientId == clientId);
            if (alreadyProvider)
                return Conflict(new ApiResponse(StatusCodes.Status409Conflict, "You already have a provider request."));

            var region = await unitOfWork.Repository<Region>().GetByIdAsync(providerFromUserDTO.RegionId);
            if (region == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid Region"));

            if (region.GovernorateId != providerFromUserDTO.GovernorateId)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Region does not belong to the selected governorate"));

            client.GovernorateId = providerFromUserDTO.GovernorateId;
            client.RegionId = providerFromUserDTO.RegionId;

            var provider = new Provider
            {
                ClientId = clientId,
                StartedAt = DateHelper.GetTodayInEgypt(),
                VerificationStatus = VerificationStatus.Pending,
                Isverified = false,
                Rating = null,
                RatingSum = 0,
                ReviewsCount = 0,
                JobsCount = 0,
                Nickname = providerFromUserDTO.Nickname,
                Bio = providerFromUserDTO.Bio
            };

            try
            {
                unitOfWork.Repository<Client>().Update(client);
                await unitOfWork.Repository<Provider>().AddAsync(provider);
                await unitOfWork.CompleteAsync();
                return StatusCode(StatusCodes.Status201Created, new ApiResponse(StatusCodes.Status201Created, "Your request to become a provider has been submitted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, $"An error occurred while processing the request: {ex.Message}"));
            }
        }
        #endregion


        #region Verify Provider
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

            var documents = provider.ProviderDocuments ?? new List<ProviderDocument>();
            var hasDocuments = documents.Count > 0;
            var distinctDocumentTypes = documents.Select(d => d.DocumentType).Distinct().Count();
            var allDocumentsApproved = documents.Count == 3 && distinctDocumentTypes == 3 && documents.All(d => d.IsApproved == true);

            if (isVerified && (!hasBaseLocation || !hasServices || !hasDocuments || !allDocumentsApproved))
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
        #endregion

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
            var region = await unitOfWork.Repository<Region>().GetByIdAsync(dto.RegionId);
            if (!governorateExists || region == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid Governorate or Region"));

            if (region.GovernorateId != dto.GovernorateId)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Region does not belong to the selected governorate"));

            providerFromDb.Nickname = dto.Nickname;
            providerFromDb.Bio = dto.Bio;

            var client = await unitOfWork.Repository<Client>().GetByIdAsync(clientId);
            if (client == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Client not found"));

            client.GovernorateId = dto.GovernorateId;
            client.RegionId = dto.RegionId;

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
                unitOfWork.Repository<Client>().Update(client);
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

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpGet("get-provider-profile/{providerId:int}")]
        public async Task<ActionResult<ProviderProfileDTO>> GetProviderProfile(int providerId)
        {
            var provider = await unitOfWork.Repository<Provider>()
                .GetByIdWithIncludesAsync(
                    providerId,
                    p => p.Client,
                    p => p.BaseLocation!,
                    p => p.ProviderServices!);

            if (provider == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var providerServices = await unitOfWork.Repository<ProviderService>()
                .GetManyByConditionAsync(ps => ps.ProviderId == providerId, ps => ps.Service!) ?? new List<ProviderService>();

            var services = providerServices
                .Select(ps => ps.Service)
                .Where(s => s != null)
                .ToList();

            var client = await unitOfWork.Repository<Client>()
                .GetByIdWithIncludesAsync(provider.ClientId, c => c.phoneNumbers!) ?? provider.Client;

            var completedJobsCount = await unitOfWork.Repository<ServiceRequest>()
                .GetCountAsync(new BaseSpecifications<ServiceRequest>(sr =>
                    sr.ProviderId == providerId && sr.RequestStatus == RequestStatus.Completed));

            var dto = mapper.Map<ProviderProfileDTO>(provider);
            dto.Services = mapper.Map<IReadOnlyList<ServiceDTO>>(services, opt => opt.Items["lang"] = "ar");
            dto.PhoneNumbers = client.phoneNumbers?.Select(p => p.PhoneNumber).ToList();
            dto.JobsCount = completedJobsCount;

            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpGet("get-my-provider-profile")]
        public async Task<ActionResult<ProviderProfileDTO>> GetMyProviderProfile()
        {
            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out var providerId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

            var provider = await unitOfWork.Repository<Provider>()
                .GetByIdWithIncludesAsync(
                    providerId,
                    p => p.Client,
                    p => p.BaseLocation!,
                    p => p.ProviderServices!);

            if (provider == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var providerServices = await unitOfWork.Repository<ProviderService>()
                .GetManyByConditionAsync(ps => ps.ProviderId == providerId, ps => ps.Service!) ?? new List<ProviderService>();

            var services = providerServices
                .Select(ps => ps.Service)
                .Where(s => s != null)
                .ToList();

            var client = await unitOfWork.Repository<Client>()
                .GetByIdWithIncludesAsync(provider.ClientId, c => c.phoneNumbers!) ?? provider.Client;

            var completedJobsCount = await unitOfWork.Repository<ServiceRequest>()
                .GetCountAsync(new BaseSpecifications<ServiceRequest>(sr =>
                    sr.ProviderId == providerId && sr.RequestStatus == RequestStatus.Completed));

            var dto = mapper.Map<ProviderProfileDTO>(provider);
            dto.Services = mapper.Map<IReadOnlyList<ServiceDTO>>(services, opt => opt.Items["lang"] = "ar");
            dto.PhoneNumbers = client.phoneNumbers?.Select(p => p.PhoneNumber).ToList();
            dto.JobsCount = completedJobsCount;

            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Admin))]
        [HttpGet("underReview-providers")]
        public async Task<ActionResult<IReadOnlyList<ProviderApprovalDTO>>> GetUnderReviewProviders()
        {
            var providers = await unitOfWork.Repository<Provider>()
                .GetManyByConditionAsync(
                    p => p.VerificationStatus == VerificationStatus.UnderReview,
                    p => p.Client,
                    p => p.ProviderDocuments!) ?? new List<Provider>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var result = providers.Select(p =>
            {
                var dob = p.Client.DateOfBirth;
                var age = today.Year - dob.Year;
                if (dob > today.AddYears(-age)) age--;

                return new ProviderApprovalDTO
                {
                    Id = p.Id,
                    Name = $"{p.Client.FirstName} {p.Client.LastName}".Trim(),
                    Age = age,
                    PictureUrl = p.Client.PictureUrl,
                    Documents = (p.ProviderDocuments ?? new List<ProviderDocument>())
                        .Select(d => new ProviderDocumentItemDTO
                        {
                            Id = d.Id,
                            Url = d.DocumentUrl
                        })
                        .ToList()
                };
            }).ToList();

            return Ok(result);
        }
    }
}
