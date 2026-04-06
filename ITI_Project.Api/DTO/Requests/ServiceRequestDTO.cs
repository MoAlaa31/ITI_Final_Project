using ITI_Project.Api.DTO.Location;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;

namespace ITI_Project.Api.DTO.Requests
{
    public class ServiceRequestDTO
    {
        public int Id { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal? FinalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PreferredTime { get; set; }
        public int ClientId { get; set; }
        public int? ProviderId { get; set; }
        public ServiceRequestLocationDTO? ServiceRequestLocation { get; set; }
        public int ServiceId { get; set; }
    }
}
