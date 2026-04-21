using ITI_Project.Api.Hubs.Interfaces;
using ITI_Project.Core.Constants;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ITI_Project.Api.Hubs
{
    public class NotificationHub : Hub<INotification>
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var clientId = Context.User?.FindFirstValue(Identifiers.ClientId);

            _logger.LogInformation("SignalR: Client connected with ClientId: {ClientId}", clientId);

            if (!string.IsNullOrEmpty(clientId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{clientId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var clientId = Context.User?.FindFirstValue(Identifiers.ClientId);

            if (!string.IsNullOrEmpty(clientId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{clientId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}