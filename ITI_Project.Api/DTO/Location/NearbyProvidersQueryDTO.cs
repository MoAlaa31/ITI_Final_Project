using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Location
{
    public class NearbyProvidersQueryDTO
    {
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
        public double RadiusKm { get; set; } = 10;

        [Required]
        public int ServiceId { get; set; }
    }
}