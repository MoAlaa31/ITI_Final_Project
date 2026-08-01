using ITI_Project.Core;
using ITI_Project.Core.Common;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Errors;
using ITI_Project.Core.IServices;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Users;
using ITI_Project.Services.User.DTOs.ClientDTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Services.User.UserServices.ClientService
{
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IFileStorageService fileStorageService;

        public ClientService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
        {
            this.unitOfWork = unitOfWork;
            this.fileStorageService = fileStorageService;
        }
        public async Task<ServiceResult<Client>> GetClientProfileAsync(int clientId)
        {
            var client = await unitOfWork.Repository<Client>().GetByIdWithIncludesAsync(clientId, c => c.phoneNumbers!);
            if (client is null)
            {
                return ServiceResult<Client>.Failure(
                    new Error(
                        "Client.NotFound",
                        "Client not found",
                        HttpStatusCode.NotFound));
            }
            return ServiceResult<Client>.Success(client);
        }

        public async Task<ServiceResult<Client>> UpdateClientProfileAsync(int clientId, ServiceUpdateClientProfileDTO clientUpdateDTO)
        {
            var client = await unitOfWork.Repository<Client>()
                .GetByIdWithIncludesAsync(clientId, c => c.phoneNumbers!);

            if (client is null)
            {
                return ServiceResult<Client>.Failure(
                    new Error(
                        "Client.NotFound",
                        "Client not found",
                        HttpStatusCode.NotFound));
            }

            // Ensure the selected region belongs to the selected governorate
            var region = await unitOfWork.Repository<Region>().GetByIdAsync(clientUpdateDTO.RegionId);
            if (region is null)
                return ServiceResult<Client>.Failure(
                    new Error(
                        "Region.Invalid",
                        "Invalid Region",
                        HttpStatusCode.BadRequest));

            if (region.GovernorateId != clientUpdateDTO.GovernorateId)
                return ServiceResult<Client>.Failure(
                    new Error(
                        "Region.Mismatch",
                        "Region does not belong to the selected governorate",
                        HttpStatusCode.BadRequest));

            UpdateBasicInformation(client, clientUpdateDTO);

            if (clientUpdateDTO.Picture != null)
            {
                var uploadResult = await fileStorageService.UploadFileAsync(
                    new FileUploadRequest
                    {
                        File = clientUpdateDTO.Picture.Content,
                        OriginalFileName = clientUpdateDTO.Picture.FileName,
                        FolderName = "client-pictures",
                        GivenName = client.FirstName,
                        NameId = client.Id.ToString()
                    });

                if (!uploadResult.Success)
                    return ServiceResult<Client>.Failure(
                        new Error(
                            "Client.PictureUploadFailed",
                            "Failed to upload client picture",
                            HttpStatusCode.BadRequest));

                if (!string.IsNullOrWhiteSpace(client.PictureUrl))
                    fileStorageService.DeleteFile(client.PictureUrl);

                client.PictureUrl = uploadResult.FilePath;
            }

            await UpdatePhoneNumbersAsync(client, clientUpdateDTO.PhoneNumbers);

            unitOfWork.Repository<Client>().Update(client);

            await unitOfWork.CompleteAsync();

            return ServiceResult<Client>.Success(client);
        }

        private async Task UpdatePhoneNumbersAsync(
            Client client,
            List<string>? phoneNumbers)
        {
            if (phoneNumbers is null)
                return;

            var existingNumbers =
                client.phoneNumbers ?? new List<UserPhoneNumber>();

            if (existingNumbers.Count > 0)
            {
                unitOfWork.Repository<UserPhoneNumber>()
                    .DeleteRange(existingNumbers);
            }

            var normalizedNumbers = phoneNumbers
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
            {
                await unitOfWork.Repository<UserPhoneNumber>()
                    .AddRangeAsync(newNumbers);
            }

            client.phoneNumbers = newNumbers;
        }

        private static void UpdateBasicInformation(
            Client client,
            ServiceUpdateClientProfileDTO dto)
        {
            client.FirstName = dto.FirstName;
            client.LastName = dto.LastName;
            client.GovernorateId = dto.GovernorateId;
            client.RegionId = dto.RegionId;
            client.Gender = dto.Gender.GetValueOrDefault(Gender.Male);
            client.DateOfBirth = dto.DateOfBirth.GetValueOrDefault();
        }
    }
}
