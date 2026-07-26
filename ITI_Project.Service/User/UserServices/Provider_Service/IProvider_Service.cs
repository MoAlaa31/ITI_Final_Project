using ITI_Project.Core.Common;
using ITI_Project.Core.Models.Users;
using ITI_Project.Services.User.DTOs.ProviderDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI_Project.Services.User.UserServices.Provider_Service
{
    public interface IProvider_Service
    {
        Task<ServiceResult<Provider>> RequestToBeProvider(int clientId, ServiceProviderFromUserDTO providerFromUserDTO);
        Task<ServiceResult> VerifyProvider(int providerId, bool isVerified);
        Task<ServiceResult<Provider>> UpdateProviderProfile(int clientId, ServiceProviderProfileUpdateDTO dto);
        Task<ServiceResult<Provider>> GetProviderByIdWithIncludes(int providerId);
        Task<ServiceResult<Provider>> GetProviderByClientIdWithIncludes(int clientId);
        Task<ServiceResult<IReadOnlyList<Provider>>> GetUnderReviewProviders();
        Task<ServiceResult> AddCredits(int providerId, int credits);
        Task<ServiceResult<ServiceProviderProfileData>> GetProviderProfileDataById(int providerId);
        Task<ServiceResult<ServiceProviderProfileData>> GetProviderProfileDataByClientId(int clientId);
    }
}
