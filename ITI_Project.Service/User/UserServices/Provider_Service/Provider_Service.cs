using ITI_Project.Core;
using ITI_Project.Core.Common;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Errors;
using ITI_Project.Core.Helpers;
using ITI_Project.Core.Models.Identity;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Services;
using ITI_Project.Core.Models.Users;
using ITI_Project.Core.Specifications;
using ITI_Project.Services.User.DTOs.ProviderDTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ITI_Project.Services.User.UserServices.Provider_Service
{
    public class Provider_Service : IProvider_Service
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<AppUser> userManager;

        public Provider_Service(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }

        public async Task<ServiceResult<Provider>> RequestToBeProvider(int clientId, ServiceProviderFromUserDTO providerFromUserDTO)
        {
            var client = await unitOfWork.Repository<Client>().GetByIdAsync(clientId);
            if (client == null)
                return ServiceResult<Provider>.Failure(new Error("Client.NotFound", "Client not found", HttpStatusCode.NotFound));

            var alreadyProvider = await unitOfWork.Repository<Provider>().AnyAsync(p => p.ClientId == clientId);
            if (alreadyProvider)
                return ServiceResult<Provider>.Failure(new Error("Provider.AlreadyExists", "You already have a provider request.", HttpStatusCode.Conflict));

            var region = await unitOfWork.Repository<Region>().GetByIdAsync(providerFromUserDTO.RegionId);
            if (region == null)
                return ServiceResult<Provider>.Failure(new Error("Region.Invalid", "Invalid Region", HttpStatusCode.BadRequest));

            if (region.GovernorateId != providerFromUserDTO.GovernorateId)
                return ServiceResult<Provider>.Failure(new Error("Region.Invalid", "Region does not belong to the selected governorate", HttpStatusCode.BadRequest));

            client.GovernorateId = providerFromUserDTO.GovernorateId;
            client.RegionId = providerFromUserDTO.RegionId;

            var provider = new Provider
            {
                ClientId = clientId,
                StartedAt = DateHelper.GetNowInEgypt(),
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
                return ServiceResult<Provider>.Success(provider);
            }
            catch
            {
                return ServiceResult<Provider>.Failure(new Error("Provider.RequestFailed", "Failed to request provider", HttpStatusCode.InternalServerError));
            }
        }

        public async Task<ServiceResult> VerifyProvider(int providerId, bool isVerified)
        {
            var provider = await unitOfWork.Repository<Provider>()
                .GetByIdWithIncludesAsync(
                    providerId,
                    p => p.ProviderServices!,
                    p => p.BaseLocation!,
                    p => p.ProviderDocuments!,
                    p => p.Client);

            if (provider == null)
                return ServiceResult<Provider>.Failure(new Error("Provider.NotFound", "Provider not found", HttpStatusCode.NotFound));

            var hasBaseLocation = provider.BaseLocation != null;
            var hasServices = provider.ProviderServices != null && provider.ProviderServices.Any();

            var documents = provider.ProviderDocuments ?? new List<ProviderDocument>();
            var hasDocuments = documents.Count > 0;
            var distinctDocumentTypes = documents.Select(d => d.DocumentType).Distinct().Count();
            var allDocumentsApproved = documents.Count == 3 && distinctDocumentTypes == 3 && documents.All(d => d.IsApproved == true);

            if (isVerified && (!hasBaseLocation || !hasServices || !hasDocuments || !allDocumentsApproved))
                return ServiceResult<Provider>.Failure(
                    new Error("Provider.InvalidData", "Provider cannot be verified. Ensure that the provider has a base location, at least one service, and all documents are approved.", HttpStatusCode.BadRequest));

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
            return ServiceResult.Success();
        }

        public async Task<ServiceResult<Provider>> UpdateProviderProfile(int clientId, ServiceProviderProfileUpdateDTO dto)
        {
            var providerFromDb = await unitOfWork.Repository<Provider>().GetByConditionAsync(p => p.ClientId == clientId);
            if (providerFromDb == null)
                return ServiceResult<Provider>.Failure(new Error("Provider.NotFound", "Provider not found", HttpStatusCode.NotFound));

            var governorateExists = await unitOfWork.Repository<Governorate>().AnyAsync(g => g.Id == dto.GovernorateId);
            var region = await unitOfWork.Repository<Region>().GetByIdAsync(dto.RegionId);
            if (!governorateExists || region == null)
                return ServiceResult<Provider>.Failure(new Error("Location.Invalid", "Invalid Governorate or Region", HttpStatusCode.BadRequest));

            if (region.GovernorateId != dto.GovernorateId)
                return ServiceResult<Provider>.Failure(new Error("Location.Invalid", "Region does not belong to the selected governorate", HttpStatusCode.BadRequest));

            providerFromDb.Nickname = dto.Nickname;
            providerFromDb.Bio = dto.Bio;

            var client = await unitOfWork.Repository<Client>().GetByIdAsync(clientId);
            if (client == null)
                return ServiceResult<Provider>.Failure(new Error("Client.NotFound", "Client not found", HttpStatusCode.NotFound));

            client.GovernorateId = dto.GovernorateId;
            client.RegionId = dto.RegionId;

            if (dto.BaseLocation != null)
            {
                var baseLocation = await unitOfWork.Repository<BaseLocation>()
                    .GetByConditionAsync(bl => bl.ProviderId == providerFromDb.Id);

                if (baseLocation != null)
                {
                    baseLocation.Latitude = dto.BaseLocation.Latitude;
                    baseLocation.Longitude = dto.BaseLocation.Longitude;
                    baseLocation.AddressText = dto.BaseLocation.AddressText;
                    unitOfWork.Repository<BaseLocation>().Update(baseLocation);
                }
                else
                {
                    baseLocation = new BaseLocation
                    {
                        ProviderId = providerFromDb.Id,
                        Latitude = dto.BaseLocation.Latitude,
                        Longitude = dto.BaseLocation.Longitude,
                        AddressText = dto.BaseLocation.AddressText
                    };
                    await unitOfWork.Repository<BaseLocation>().AddAsync(baseLocation);
                }
            }

            var distinctIds = dto.ServiceIds.Distinct().ToList();
            if (distinctIds.Count > 2)
                return ServiceResult<Provider>.Failure(new Error("Service.Limit", "A provider can only offer up to 2 services", HttpStatusCode.BadRequest));

            if (distinctIds.Count > 0)
            {
                var services = await unitOfWork.Repository<Service>().GetManyByConditionAsync(s => distinctIds.Contains(s.Id)) ?? new List<Service>();
                if (services.Count != distinctIds.Count)
                    return ServiceResult<Provider>.Failure(new Error("Service.Invalid", "One or more ServiceIds are invalid", HttpStatusCode.BadRequest));

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
                return ServiceResult<Provider>.Success(providerFromDb);
            }
            catch
            {
                return ServiceResult<Provider>.Failure(new Error("Provider.UpdateFailed", "An error occurred while updating provider profile", HttpStatusCode.InternalServerError));
            }
        }

        public async Task<ServiceResult<Provider>> GetProviderByIdWithIncludes(int providerId)
        {
            var provider = await unitOfWork.Repository<Provider>()
                .GetByIdWithIncludesAsync(
                    providerId,
                    p => p.Client,
                    p => p.BaseLocation!,
                    p => p.ProviderServices!);

            if (provider == null)
                return ServiceResult<Provider>.Failure(new Error("Provider.NotFound", "Provider not found", HttpStatusCode.NotFound));

            return ServiceResult<Provider>.Success(provider);
        }

        public async Task<ServiceResult<Provider>> GetProviderByClientIdWithIncludes(int clientId)
        {
            var provider = await unitOfWork.Repository<Provider>()
                .GetByConditionAsync(
                    p => p.ClientId == clientId,
                    p => p.Client,
                    p => p.BaseLocation!,
                    p => p.ProviderServices!);

            if (provider == null)
                return ServiceResult<Provider>.Failure(new Error("Provider.NotFound", "Provider not found", HttpStatusCode.NotFound));

            return ServiceResult<Provider>.Success(provider);
        }

        public async Task<ServiceResult<IReadOnlyList<Provider>>> GetUnderReviewProviders()
        {
            var providers = await unitOfWork.Repository<Provider>()
                .GetManyByConditionAsync(
                    p => p.VerificationStatus == VerificationStatus.UnderReview,
                    p => p.Client,
                    p => p.ProviderDocuments!);

            var list = providers ?? new List<Provider>();
            return ServiceResult<IReadOnlyList<Provider>>.Success(list);
        }

        public async Task<ServiceResult> AddCredits(int providerId, int credits)
        {
            if (credits <= 0)
                return ServiceResult.Failure(new Error("Credits.Invalid", "Amount must be greater than zero.", HttpStatusCode.BadRequest));

            var provider = await unitOfWork.Repository<Provider>().GetByIdAsync(providerId);
            if (provider == null)
                return ServiceResult.Failure(new Error("Provider.NotFound", "Provider not found", HttpStatusCode.NotFound));

            provider.Credits += credits;
            unitOfWork.Repository<Provider>().Update(provider);
            await unitOfWork.CompleteAsync();

            return ServiceResult.Success();
        }

        public async Task<ServiceResult<ServiceProviderProfileData>> GetProviderProfileDataById(int providerId)
        {
            var provider = await unitOfWork.Repository<Provider>()
                .GetByIdWithIncludesAsync(providerId, p => p.Client, p => p.BaseLocation!, p => p.ProviderServices!);

            if (provider == null)
                return ServiceResult<ServiceProviderProfileData>.Failure(new Error("Provider.NotFound", "Provider not found", HttpStatusCode.NotFound));

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

            var data = new ServiceProviderProfileData
            {
                Provider = provider,
                Services = services!,
                PhoneNumbers = client.phoneNumbers?.Select(p => p.PhoneNumber).ToList(),
                CompletedJobsCount = completedJobsCount
            };

            return ServiceResult<ServiceProviderProfileData>.Success(data);
        }

        public async Task<ServiceResult<ServiceProviderProfileData>> GetProviderProfileDataByClientId(int clientId)
        {
            var provider = await unitOfWork.Repository<Provider>()
                .GetByConditionAsync(p => p.ClientId == clientId, p => p.Client, p => p.BaseLocation!, p => p.ProviderServices!);

            if (provider == null)
                return ServiceResult<ServiceProviderProfileData>.Failure(new Error("Provider.NotFound", "Provider not found", HttpStatusCode.NotFound));

            var providerServices = await unitOfWork.Repository<ProviderService>()
                .GetManyByConditionAsync(ps => ps.ProviderId == provider.Id, ps => ps.Service!) ?? new List<ProviderService>();

            var services = providerServices
                .Select(ps => ps.Service)
                .Where(s => s != null)
                .ToList();

            var client = await unitOfWork.Repository<Client>()
                .GetByIdWithIncludesAsync(provider.ClientId, c => c.phoneNumbers!) ?? provider.Client;

            var completedJobsCount = await unitOfWork.Repository<ServiceRequest>()
                .GetCountAsync(new BaseSpecifications<ServiceRequest>(sr =>
                    sr.ProviderId == provider.Id && sr.RequestStatus == RequestStatus.Completed));

            var data = new ServiceProviderProfileData
            {
                Provider = provider,
                Services = services!,
                PhoneNumbers = client.phoneNumbers?.Select(p => p.PhoneNumber).ToList(),
                CompletedJobsCount = completedJobsCount
            };

            return ServiceResult<ServiceProviderProfileData>.Success(data);
        }
    }
}
