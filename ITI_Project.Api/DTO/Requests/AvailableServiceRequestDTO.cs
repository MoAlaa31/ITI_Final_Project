namespace ITI_Project.Api.DTO.Requests
{
    public class AvailableServiceRequestDTO : ServiceRequestProviderDTO
    {
        public bool HasOffer { get; set; }
        public int? OfferId { get; set; }
    }
}
