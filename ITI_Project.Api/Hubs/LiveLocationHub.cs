using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Requests;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ITI_Project.Api.Hubs
{
    public class LiveLocationHub : Hub<ITrackingClient>
    {
        private readonly IUnitOfWork unitOfWork;

        public LiveLocationHub(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task JoinProviderGroup(int providerId)
        {
            var clientIdClaim = Context.User?.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                throw new HubException("ClientId claim is missing or invalid");

            var hasActiveRequest = await unitOfWork.Repository<ServiceRequest>()
                .AnyAsync(r => r.ProviderId == providerId
                           && r.ClientId == clientId
                           && (r.RequestStatus == RequestStatus.Assigned || r.RequestStatus == RequestStatus.InProgress));

            if (!hasActiveRequest)
                throw new HubException("You are not authorized to track this provider");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"provider-{providerId}");
        }
    }
}