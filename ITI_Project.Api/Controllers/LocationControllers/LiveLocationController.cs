using AutoMapper;
using ITI_Project.Api.DTO.Location;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Helpers;
using ITI_Project.Api.Hubs;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.LocationControllers
{
    public class LiveLocationController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IHubContext<LiveLocationHub, ITrackingClient> hub;

        public LiveLocationController(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<LiveLocationHub, ITrackingClient> hub)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.hub = hub;
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpPut("update-live-location")]
        public async Task<ActionResult<LiveLocationDTO>> UpdateLiveLocation([FromBody] LiveLocationUpdateDTO dto)
        {
            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out var providerId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

            var liveLocation = await unitOfWork.Repository<LiveLocation>()
                .GetByConditionAsync(l => l.ProviderId == providerId);

            var now = DateHelper.GetNowInEgypt();
            bool shouldSave = true;

            if (liveLocation is null)
            {
                liveLocation = new LiveLocation
                {
                    ProviderId = providerId,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    UpdatedAt = now
                };
                await unitOfWork.Repository<LiveLocation>().AddAsync(liveLocation);
            }
            else
            {
                const double MIN_DISTANCE_METERS = 10;   // tweak
                const int MIN_SECONDS = 5;               // tweak
                const int SAVE_INTERVAL_SECONDS = 10;    // tweak

                //Movement too small(< 10m & < 5s)
                //   → ignore completely

                //Movement small but time passed
                //   → send SignalR
                //   → skip DB

                //Movement big or time big
                //   → send SignalR
                //   → save DB

                var distance = CalculateDistanceMeters(
                    liveLocation.Latitude,
                    liveLocation.Longitude,
                    dto.Latitude,
                    dto.Longitude
                );

                var timeDiff = (now - liveLocation.UpdatedAt).TotalSeconds;

                if (distance < MIN_DISTANCE_METERS && timeDiff < MIN_SECONDS)
                {
                    // Ignore update if too small movement in short time
                    return NoContent();
                }

                // update in memory entity for broadcasting
                liveLocation.Latitude = dto.Latitude;
                liveLocation.Longitude = dto.Longitude;

                // reduce database writes by only saving if significant movement or enough time has passed
                if (timeDiff < SAVE_INTERVAL_SECONDS)
                {
                    shouldSave = false;
                }
                else
                {
                    liveLocation.UpdatedAt = now;
                    unitOfWork.Repository<LiveLocation>().Update(liveLocation);
                }
            }

            if (shouldSave)
            {
                await unitOfWork.CompleteAsync();
            }

            var result = mapper.Map<LiveLocationDTO>(liveLocation);

            await hub.Clients.Group($"provider-{providerId}")
                .ReceiveLocation(result);

            return Ok(result);
        }

        [Authorize(Roles = $"{nameof(UserRoleType.Client)},{nameof(UserRoleType.Provider)},{nameof(UserRoleType.Admin)}")]
        [HttpGet("get-live-location/{providerId:int}")]
        public async Task<ActionResult<LiveLocationDTO>> GetLiveLocation(int providerId)
        {
            if (User.IsInRole(nameof(UserRoleType.Client)))
            {
                var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
                if (!int.TryParse(clientIdClaim, out var clientId))
                    return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

                var hasActiveRequest = await unitOfWork.Repository<ServiceRequest>()
                    .AnyAsync(r => r.ProviderId == providerId
                               && r.ClientId == clientId
                               && (r.RequestStatus == RequestStatus.Assigned || r.RequestStatus == RequestStatus.InProgress));

                if (!hasActiveRequest)
                    return Forbid();
            }

            var liveLocation = await unitOfWork.Repository<LiveLocation>()
                .GetByConditionAsync(l => l.ProviderId == providerId);

            if (liveLocation is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Live Location not found"));

            return Ok(mapper.Map<LiveLocationDTO>(liveLocation));
        }


        public static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371000; // meters
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRad(double val) => val * (Math.PI / 180);
    }
}
