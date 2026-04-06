using AutoMapper;
using ITI_Project.Api.DTO.Location;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Services;
using ITI_Project.Core.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.LocationControllers
{
    public class BaseLocationController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public BaseLocationController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [Authorize(Roles = $"{nameof(UserRoleType.Client)},{nameof(UserRoleType.Provider)},{nameof(UserRoleType.Admin)}")]
        [HttpGet("get-base-location/{baseLocationId:int}")]
        public async Task<ActionResult<BaseLocationDTO>> GetBaseLocation(int baseLocationId)
        {
            var baseLocation = await unitOfWork.Repository<BaseLocation>().GetByIdAsync(baseLocationId);
            if (baseLocation == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Base Location not found"));

            var baseLocationDTO = mapper.Map<BaseLocationDTO>(baseLocation);
            return Ok(baseLocationDTO);
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpPut("update-base-location")]
        public async Task<ActionResult<BaseLocationDTO>> UpdateBaseLocation(BaseLocationCreateDTO baseLocationFromUserDTO)
        {
            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out var providerId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

            var providerExists = await unitOfWork.Repository<Provider>().AnyAsync(p => p.Id == providerId);
            if (!providerExists)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Provider not found"));

            var baseLocation = await unitOfWork.Repository<BaseLocation>().GetByConditionAsync(bl => bl.ProviderId == providerId);

            if (baseLocation != null)
            {
                mapper.Map(baseLocationFromUserDTO, baseLocation);
                unitOfWork.Repository<BaseLocation>().Update(baseLocation);
                await unitOfWork.CompleteAsync();
                var dto = mapper.Map<BaseLocationDTO>(baseLocation);
                return Ok(dto);
            }
            else
            {
                baseLocation = mapper.Map<BaseLocation>(baseLocationFromUserDTO);
                baseLocation.ProviderId = providerId;
                await unitOfWork.Repository<BaseLocation>().AddAsync(baseLocation);
                await unitOfWork.CompleteAsync(); 
                var dto = mapper.Map<BaseLocationDTO>(baseLocation);
                return CreatedAtAction(nameof(GetBaseLocation), new { baseLocationId = baseLocation.Id }, dto);
            }


        }
        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpGet("nearby-providers")]
        public async Task<ActionResult<IReadOnlyList<NearbyProviderDTO>>> GetNearbyProviders([FromQuery] NearbyProvidersQueryDTO query)
        {
            var serviceExists = await unitOfWork.Repository<Service>().AnyAsync(s => s.Id == query.ServiceId);
            if (!serviceExists)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid service"));

            var radius = Math.Max(1, query.RadiusKm);

            var deltaLat = radius / 111.32;
            var deltaLng = radius / (111.32 * Math.Cos(query.Latitude * Math.PI / 180));

            var minLat = query.Latitude - deltaLat;
            var maxLat = query.Latitude + deltaLat;
            var minLng = query.Longitude - deltaLng;
            var maxLng = query.Longitude + deltaLng;

            var locations = await unitOfWork.Repository<BaseLocation>()
                .GetManyByConditionAsync(
                    bl => bl.Latitude >= minLat &&
                          bl.Latitude <= maxLat &&
                          bl.Longitude >= minLng &&
                          bl.Longitude <= maxLng,
                    bl => bl.Provider,
                    bl => bl.Provider.Client,
                    bl => bl.Provider.ProviderServices!) ?? new List<BaseLocation>();

            var nearby = locations
                .Where(bl =>
                    bl.Provider?.ProviderServices?.Any(ps => ps.ServiceId == query.ServiceId) == true &&
                    GetDistanceKm(query.Latitude, query.Longitude, bl.Latitude, bl.Longitude) <= radius)
                .Select(bl => new NearbyProviderDTO
                {
                    Id = bl.Provider!.Id,
                    Name = $"{bl.Provider.Client.FirstName} {bl.Provider.Client.LastName}".Trim(),
                    Profession = query.ServiceId,
                    Experience = bl.Provider.Bio,
                    Rating = bl.Provider.Rating ?? 0,
                    Distance = GetDistanceKm(query.Latitude, query.Longitude, bl.Latitude, bl.Longitude),
                    Status = "متاح الآن",
                    Position = new PositionDTO { Lat = bl.Latitude, Lng = bl.Longitude },
                    Avatar = bl.Provider.Client.PictureUrl
                })
                .ToList();

            return Ok(nearby);
        }

        private static double GetDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180);
    }
}
