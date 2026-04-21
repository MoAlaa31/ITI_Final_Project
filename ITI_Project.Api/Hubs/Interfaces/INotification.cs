namespace ITI_Project.Api.Hubs.Interfaces
{
    public interface INotification
    {
        Task ReceiveNotification(object notification);
    }
}
