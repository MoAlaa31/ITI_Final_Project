using ITI_Project.Api.DTO.Location;

namespace ITI_Project.Api.Hubs
{
    public interface ITrackingClient
    {
        Task ReceiveLocation(LiveLocationDTO location);
    }
}
