namespace ITI_Project.Api.DTO.Requests
{
    public class ServiceRequestByIdDTO : ServiceRequestDTO
    {
        public bool IsReviewed { get; set; }
        public int? ReviewId { get; set; }
    }
}
