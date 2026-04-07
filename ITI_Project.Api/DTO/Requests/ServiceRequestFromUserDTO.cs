using ITI_Project.Api.DTO.Location;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Requests
{
    public class ServiceRequestFromUserDTO
    {
        [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters.")]
        [Required(ErrorMessage = "Description is required.")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Latitude is required.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required.")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "ServiceId is required.")]
        public int ServiceId { get; set; }

        public List<IFormFile>? Images { get; set; }
    }
}
