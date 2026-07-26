using AutoMapper;
using ITI_Project.Api.DTO.Services;
using ITI_Project.Api.DTO.Users;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Filters;
using ITI_Project.Api.Helpers;
using ITI_Project.Core;
using ITI_Project.Core.Common;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Helpers;
using ITI_Project.Core.Models.Identity;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Services;
using ITI_Project.Core.Models.Users;
using ITI_Project.Core.Specifications;
using ITI_Project.Services.User.DTOs.ProviderDTOs;
using ITI_Project.Services.User.UserServices.Provider_Service;
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
        private readonly IProvider_Service providerService;

        public ProviderController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager, IProvider_Service providerService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
            this.providerService = providerService;
        }

        #region Request to be provider
        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPost("BeProvider")]
        public async Task<ActionResult> RequestToBeProvider(ProviderDTO providerFromUserDTO)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var providerFromUser = new ServiceProviderFromUserDTO
            {
                Bio = providerFromUserDTO.Bio,
                Nickname = providerFromUserDTO.Nickname,
                GovernorateId = providerFromUserDTO.GovernorateId,
                RegionId = providerFromUserDTO.RegionId
            };

            var result = await providerService.RequestToBeProvider(clientId, providerFromUser);
            if (!result.IsSuccess)
                return StatusCode((int)result.Error.StatusCode, new ApiResponse((int)result.Error.StatusCode, result.Error.Message));

            return Created("", new ApiResponse(StatusCodes.Status201Created, "Your request to become a provider has been submitted successfully."));
        }
        #endregion


        #region Verify Provider
        [Authorize(Roles = nameof(UserRoleType.Admin))]
        [HttpPatch("VerifyProvider/{providerId:int}")]
        public async Task<ActionResult> VerifyProvider(int providerId, [FromQuery] bool isVerified)
        {
            var result = await providerService.VerifyProvider(providerId, isVerified);
            if (!result.IsSuccess)
                return StatusCode((int)result.Error.StatusCode, new ApiResponse((int)result.Error.StatusCode, result.Error.Message));

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

            var serviceDto = new ServiceProviderProfileUpdateDTO
            {
                Bio = dto.Bio,
                Nickname = dto.Nickname,
                GovernorateId = dto.GovernorateId,
                RegionId = dto.RegionId,
                ServiceIds = dto.ServiceIds ?? new List<int>(),
                BaseLocation = dto.BaseLocation == null ? null : new ServiceBaseLocationDTO
                {
                    Latitude = dto.BaseLocation.Latitude,
                    Longitude = dto.BaseLocation.Longitude,
                    AddressText = dto.BaseLocation.AddressText
                }
            };

            var result = await providerService.UpdateProviderProfile(clientId, serviceDto);
            if (!result.IsSuccess)
                return StatusCode((int)result.Error.StatusCode, new ApiResponse((int)result.Error.StatusCode, result.Error.Message));

            var response = mapper.Map<ProviderDTO>(result.Data);
            return Ok(response);
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpGet("get-provider-profile/{providerId:int}")]
        public async Task<ActionResult<ProviderProfileDTO>> GetProviderProfile(int providerId)
        {
            var result = await providerService.GetProviderProfileDataById(providerId);
            if (!result.IsSuccess)
                return StatusCode((int)result.Error.StatusCode, new ApiResponse((int)result.Error.StatusCode, result.Error.Message));

            var data = result.Data;
            var dto = mapper.Map<ProviderProfileDTO>(data.Provider);
            dto.Services = mapper.Map<IReadOnlyList<ServiceDTO>>(data.Services, opt => opt.Items["lang"] = "ar");
            dto.PhoneNumbers = data.PhoneNumbers;
            dto.JobsCount = data.CompletedJobsCount;

            return Ok(dto);
        }

        [Authorize(Roles = $"{nameof(UserRoleType.Client)}, {nameof(UserRoleType.Provider)}")]
        [HttpGet("get-my-provider-profile")]
        public async Task<ActionResult<ProviderProfilePrivateDTO>> GetMyProviderProfile()
        {
            ServiceProviderProfileData? data = null;
            ServiceResult<ServiceProviderProfileData> getResult;

            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (int.TryParse(providerIdClaim, out var providerId))
            {
                getResult = await providerService.GetProviderProfileDataById(providerId);
                if (!getResult.IsSuccess)
                    return StatusCode((int)getResult.Error.StatusCode, new ApiResponse((int)getResult.Error.StatusCode, getResult.Error.Message));

                data = getResult.Data;
            }
            else
            {
                var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
                if (!int.TryParse(clientIdClaim, out var clientId))
                    return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

                getResult = await providerService.GetProviderProfileDataByClientId(clientId);
                if (!getResult.IsSuccess)
                    return StatusCode((int)getResult.Error.StatusCode, new ApiResponse((int)getResult.Error.StatusCode, getResult.Error.Message));

                data = getResult.Data;
            }

            if (data == null || data.Provider == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var dto = mapper.Map<ProviderProfilePrivateDTO>(data.Provider);
            dto.Services = mapper.Map<IReadOnlyList<ServiceDTO>>(data.Services, opt => opt.Items["lang"] = "ar");
            dto.PhoneNumbers = data.PhoneNumbers;
            dto.JobsCount = data.CompletedJobsCount;
            dto.Credits = data.Provider.Credits;

            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Admin))]
        [HttpGet("underReview-providers")]
        public async Task<ActionResult<IReadOnlyList<ProviderApprovalDTO>>> GetUnderReviewProviders()
        {
            var result = await providerService.GetUnderReviewProviders();
            if (!result.IsSuccess)
                return StatusCode((int)result.Error.StatusCode, new ApiResponse((int)result.Error.StatusCode, result.Error.Message));

            var providers = result.Data ?? new List<Provider>();

            var today = DateHelper.GetTodayInEgypt();

            var response = providers.Select(p =>
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

            return Ok(response);
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpPost("add-credits")]
        public async Task<ActionResult> AddCredits([FromBody] AddCreditsDTO dto)
        {
            if (dto.Credits <= 0)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Amount must be greater than zero."));

            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out var providerId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

            var result = await providerService.AddCredits(providerId, dto.Credits);
            if (!result.IsSuccess)
                return StatusCode((int)result.Error.StatusCode, new ApiResponse((int)result.Error.StatusCode, result.Error.Message));

            // Fetch provider for balance — kept as a simple read but via service methods already present:
            var providerResult = await providerService.GetProviderByIdWithIncludes(providerId);
            var balance = providerResult.IsSuccess ? providerResult.Data.Credits : 0;
            return Ok(new ApiResponse(StatusCodes.Status200OK, $"Credits added successfully. New balance: {balance}"));
        }
    }
}
