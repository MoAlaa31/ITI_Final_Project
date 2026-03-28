using AutoMapper;
using ITI_Project.Api.DTO.Location;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;
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
        [HttpPost("add-base-location")]
        public async Task<ActionResult<BaseLocationDTO>> AddBaseLocation(BaseLocationCreateDTO baseLocationFromUserDTO)
        {
            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out var providerId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

            var providerExists = await unitOfWork.Repository<Provider>().AnyAsync(p => p.Id == providerId);
            if (!providerExists)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Provider not found"));

            var existingBaseLocation = await unitOfWork.Repository<BaseLocation>().AnyAsync(bl => bl.ProviderId == providerId);
            if (existingBaseLocation)
                return Conflict(new ApiResponse(StatusCodes.Status409Conflict, "Base Location already exists for this provider"));

            var baseLocation = mapper.Map<BaseLocation>(baseLocationFromUserDTO);
            baseLocation.ProviderId = providerId;

            await unitOfWork.Repository<BaseLocation>().AddAsync(baseLocation);
            await unitOfWork.CompleteAsync();

            var dto = mapper.Map<BaseLocationDTO>(baseLocation);
            return CreatedAtAction(nameof(GetBaseLocation), new { baseLocationId = baseLocation.Id }, dto);
        }
    }
}
