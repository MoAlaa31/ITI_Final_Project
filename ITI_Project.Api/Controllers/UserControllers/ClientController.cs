using AutoMapper;
using ITI_Project.Api.DTO.Users;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.IServices;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.UserControllers
{
    public class ClientController : BaseApiController
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IFileStorageService fileStorageService;

        public ClientController(IMapper mapper, IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.fileStorageService = fileStorageService;
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpGet("get-client-profile")]
        public async Task<ActionResult<ClientDTO>> GetClientProfile()
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var client = await unitOfWork.Repository<Client>().GetByIdWithIncludesAsync(clientId, c => c.phoneNumbers!);
            if (client == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Client not found"));

            return Ok(mapper.Map<ClientDTO>(client));
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPut("update-client-profile")]
        public async Task<ActionResult<ClientDTO>> UpdateClientProfile([FromForm] ClientUpdateDTO clientUpdateDTO)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var client = await unitOfWork.Repository<Client>()
                .GetByIdWithIncludesAsync(clientId, c => c.phoneNumbers!);

            if (client == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Client not found"));

            mapper.Map(clientUpdateDTO, client);
            // Ensure the selected region belongs to the selected governorate
            var region = await unitOfWork.Repository<Region>().GetByIdAsync(clientUpdateDTO.RegionId);
            if (region == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid Region"));

            if (region.GovernorateId != clientUpdateDTO.GovernorateId)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Region does not belong to the selected governorate"));

            client.GovernorateId = clientUpdateDTO.GovernorateId;
            client.RegionId = clientUpdateDTO.RegionId;

            if (clientUpdateDTO.Picture != null)
            {
                var uploadResult = await fileStorageService.UploadFileAsync(
                    clientUpdateDTO.Picture.OpenReadStream(),
                    "client-pictures",
                    clientUpdateDTO.Picture.FileName,
                    User);

                if (!uploadResult.Success)
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, uploadResult.Message));

                if (!string.IsNullOrWhiteSpace(client.PictureUrl))
                    fileStorageService.DeleteFile(client.PictureUrl);

                client.PictureUrl = uploadResult.FilePath;
            }

            if (clientUpdateDTO.PhoneNumbers != null)
            {
                var existingNumbers = client.phoneNumbers ?? new List<UserPhoneNumber>();
                if (existingNumbers.Count > 0)
                    unitOfWork.Repository<UserPhoneNumber>().DeleteRange(existingNumbers);

                var normalizedNumbers = clientUpdateDTO.PhoneNumbers
                    .Where(number => !string.IsNullOrWhiteSpace(number))
                    .Select(number => number.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var newNumbers = normalizedNumbers
                    .Select(number => new UserPhoneNumber
                    {
                        ClientId = client.Id,
                        PhoneNumber = number
                    })
                    .ToList();

                if (newNumbers.Count > 0)
                    await unitOfWork.Repository<UserPhoneNumber>().AddRangeAsync(newNumbers);

                client.phoneNumbers = newNumbers;
            }

            unitOfWork.Repository<Client>().Update(client);
            await unitOfWork.CompleteAsync();

            return Ok(mapper.Map<ClientDTO>(client));
        }
    }
}
