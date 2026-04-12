namespace ITI_Project.Api.DTO.Requests
{
    public class RequestOfferProviderDTO
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public decimal Price { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}