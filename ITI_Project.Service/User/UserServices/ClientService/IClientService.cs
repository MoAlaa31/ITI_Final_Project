using ITI_Project.Core.Common;
using ITI_Project.Core.Models.Users;
using ITI_Project.Services.User.DTOs.ClientDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Services.User.UserServices.ClientService
{
    public interface IClientService
    {
        public Task<ServiceResult<Client>> GetClientProfileAsync(int clientId);
        public Task<ServiceResult<Client>> UpdateClientProfileAsync(int clientId, ServiceUpdateClientProfileDTO clientUpdateDTO);
    }
}
