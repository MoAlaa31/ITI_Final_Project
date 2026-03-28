namespace ITI_Project.Api.DTO.Location
{
    public class BaseLocationCreateDTO
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? AddressText { get; set; }
    }
}
