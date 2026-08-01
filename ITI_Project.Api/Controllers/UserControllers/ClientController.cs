using AutoMapper;
using ITI_Project.Api.DTO.Users;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.IServices;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Users;
using ITI_Project.Services.User.DTOs;
using ITI_Project.Services.User.DTOs.ClientDTOs;
using ITI_Project.Services.User.UserServices.ClientService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.UserControllers
{
    public class ClientController : BaseApiController
    {
        private readonly IMapper mapper;
        private readonly IClientService clientService;

        public ClientController(IMapper mapper, IClientService clientService)
        {
            this.mapper = mapper;
            this.clientService = clientService;
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpGet("get-client-profile")]
        public async Task<ActionResult<ServiceClientDTO>> GetClientProfile()
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var result = await clientService.GetClientProfileAsync(clientId);

            if (result.IsFailure)
                return HandleFailure(result.Error);

            return Ok(mapper.Map<ServiceClientDTO>(result.Data));
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPut("update-client-profile")]
        public async Task<ActionResult<ServiceClientDTO>> UpdateClientProfile([FromForm] ClientUpdateDTO clientUpdateDTO)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var model = new ServiceUpdateClientProfileDTO
            {
                FirstName = clientUpdateDTO.FirstName,
                LastName = clientUpdateDTO.LastName,
                GovernorateId = clientUpdateDTO.GovernorateId,
                RegionId = clientUpdateDTO.RegionId,
                PhoneNumbers = clientUpdateDTO.PhoneNumbers,
                Gender = clientUpdateDTO.Gender,
                DateOfBirth = clientUpdateDTO.DateOfBirth,
                Picture = clientUpdateDTO.Picture is null
                    ? null
                    : new FileData
                    {
                        Content = clientUpdateDTO.Picture.OpenReadStream(),
                        FileName = clientUpdateDTO.Picture.FileName
                    },
                UserGivenName = User.FindFirstValue(ClaimTypes.GivenName),
                UserNameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            var result = await clientService.UpdateClientProfileAsync(clientId, model);

            if (result.IsFailure)
                return HandleFailure(result.Error);

            return Ok(mapper.Map<ServiceClientDTO>(result.Data));
        }
    }
}
