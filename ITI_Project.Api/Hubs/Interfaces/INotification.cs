using ITI_Project.Api.DTO.Requests;

namespace ITI_Project.Api.Hubs.Interfaces
{
    public interface INotification
    {
        Task ReceiveNotification(object notification);
        Task ReceiveRequestOffer(RequestOfferDTO offer);
    }
}
