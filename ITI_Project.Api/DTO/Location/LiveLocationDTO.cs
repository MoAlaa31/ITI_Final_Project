namespace ITI_Project.Api.DTO.Location
{
    public class LiveLocationDTO
    {
        public int ProviderId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
