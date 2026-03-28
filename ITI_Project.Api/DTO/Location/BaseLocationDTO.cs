namespace ITI_Project.Api.DTO.Location
{
    public class BaseLocationDTO
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? AddressText { get; set; }
    }
}
