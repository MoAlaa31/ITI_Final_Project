using AutoMapper;
using ITI_Project.Api.DTO.Moderation;
using ITI_Project.Api.DTO.Users;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Helpers;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Identity;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Users;
using ITI_Project.Core.Specifications;
using ITI_Project.Core.Specifications.ReportSpecs;
using ITI_Project.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.ModerationControllers
{
    public class ReportController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<AppUser> userManager;
        private readonly IMapper mapper;

        public ReportController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.mapper = mapper;
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPost("report-request")]
        public async Task<IActionResult> ReportRequest([FromBody] ReportDTO dto)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var serviceRequest = await unitOfWork.Repository<ServiceRequest>().GetByIdAsync(dto.ServiceRequestId);
            if (serviceRequest == null)
                return NotFound("Service request not found");

            if (serviceRequest.ClientId != clientId)
                return Forbid();

            if (serviceRequest.ProviderId == null)
                return BadRequest("No provider assigned to this request");

            var provider = await unitOfWork.Repository<Provider>().GetByIdAsync(serviceRequest.ProviderId.Value);
            if (provider == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var targetUserId = provider.ClientId;

            var existingReport = await unitOfWork.Repository<Report>()
                .GetByConditionAsync(r =>
                    r.ServiceRequestId == dto.ServiceRequestId &&
                    r.ReporterId == clientId);

            if (existingReport != null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "You already reported this request"));

            var report = new Report
            {
                ServiceRequestId = dto.ServiceRequestId,
                TargetUserId = targetUserId,
                ReporterId = clientId,
                Reason = dto.Reason,
                ReportType = dto.ReportType,
                Status = ReportStatus.UnderReview,
                LastUpdate = DateHelper.GetNowInEgypt()
            };

            await unitOfWork.Repository<Report>().AddAsync(report);
            await unitOfWork.CompleteAsync();

            return Ok("Report submitted successfully");
        }

        [Authorize(Roles = nameof(UserRoleType.Admin))]
        [HttpPut("resolve-report/{id:int}")]
        public async Task<IActionResult> ResolveReport(int id, [FromBody] ResolveReportDTO dto)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            // 1. Get report
            var report = await unitOfWork.Repository<Report>().GetByIdAsync(id);
            if (report == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Report not found"));

            if (report.Status != ReportStatus.UnderReview)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Report already handled"));

            if (dto.Status == ReportStatus.UnderReview)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Cannot set status to UnderReview."));

            // 2. Update report
            report.Status = dto.Status;
            report.ResolverId = clientId;
            report.LastUpdate = DateHelper.GetNowInEgypt();

            if (dto.Status == ReportStatus.Resolved)
            {
                var serviceRequest = await unitOfWork.Repository<ServiceRequest>()
                    .GetByIdAsync(report.ServiceRequestId);

                if (serviceRequest?.ProviderId != null)
                {
                    var provider = await unitOfWork.Repository<Provider>()
                        .GetByIdWithIncludesAsync(serviceRequest.ProviderId.Value, p => p.Client);

                    if (provider != null)
                    {
                        provider.VerificationStatus = VerificationStatus.Suspended;
                        provider.StartedAt = DateHelper.GetNowInEgypt();

                        unitOfWork.Repository<Provider>().Update(provider);

                        var appUser = await userManager.FindByIdAsync(provider.Client.AppUserId);
                        if (appUser != null)
                        {
                            await userManager.RemoveFromRoleAsync(appUser, nameof(UserRoleType.Provider));
                        }
                    }
                }
            }

            unitOfWork.Repository<Report>().Update(report);
            await unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Report handled successfully"));
        }

        [Authorize(Roles = nameof(UserRoleType.Admin))]
        [HttpGet("all-reports")]
        public async Task<IActionResult> GetAllReports([FromQuery] ReportSpecParams specParams)
        {
            // Set a maximum page size to prevent excessive data retrieval
            specParams.SetMaxPageSize(15);

            var countSpec = new ReportCountSpecification(specParams);
            var count = await unitOfWork.Repository<Report>().GetCountAsync(countSpec);

            var spec = new ReportWithPaginationSpecification(specParams);
            var reports = await unitOfWork.Repository<Report>().GetAllWithSpecAsync(spec) ?? new List<Report>();

            var data = mapper.Map<List<ReportFromDbDTO>>(reports);

            return Ok(new Pagination<ReportFromDbDTO>(specParams.PageIndex, specParams.PageSize, count, data));
        }

        [Authorize(Roles = nameof(UserRoleType.Admin))]
        [HttpPut("reinstate-provider/{providerId:int}")]
        public async Task<IActionResult> ReinstateProvider(int providerId)
        {
            // 1. Get provider with client
            var provider = await unitOfWork.Repository<Provider>()
                .GetByIdWithIncludesAsync(providerId, p => p.Client);

            if (provider == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            if (provider.VerificationStatus != VerificationStatus.Suspended)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Provider is not suspended"));

            // 2. Update provider status
            provider.VerificationStatus = VerificationStatus.Approved;
            unitOfWork.Repository<Provider>().Update(provider);

            // 3. Restore Provider role if needed
            var appUser = await userManager.FindByIdAsync(provider.Client.AppUserId);
            if (appUser != null)
            {
                var roles = await userManager.GetRolesAsync(appUser);
                if (!roles.Contains(nameof(UserRoleType.Provider)))
                {
                    await userManager.AddToRoleAsync(appUser, nameof(UserRoleType.Provider));
                }
            }

            await unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Provider reinstated successfully"));
        }

        [Authorize(Roles = nameof(UserRoleType.Admin))]
        [HttpGet("banned-providers-expired")]
        public async Task<IActionResult> GetBannedProvidersExpired()
        {
            //var fiveMinutesAgo = DateHelper.GetNowInEgypt().AddMinutes(-5);

            var providers = await unitOfWork.Repository<Provider>()
                .GetManyByConditionAsync(
                    p => p.VerificationStatus == VerificationStatus.Suspended,
                         //&& p.StartedAt <= fiveMinutesAgo,
                    p => p.Client
                ) ?? new List<Provider>();

             var data = mapper.Map<List<BannedProvidersDTO>>(providers);

            return Ok(data);
        }
    }
}
