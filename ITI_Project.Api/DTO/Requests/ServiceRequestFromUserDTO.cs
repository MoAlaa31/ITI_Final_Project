using ITI_Project.Api.DTO.Location;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITI_Project.Api.DTO.Requests
{
    public class ServiceRequestFromUserDTO
    {
        [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters.")]
        [Required(ErrorMessage = "Description is required.")]
        public required string Description { get; set; }
        [Range(0.0, 1000000.0, ErrorMessage = "Final Price must be a positive number.")]
        public decimal? FinalPrice { get; set; }
        public DateTime? PreferredTime { get; set; }
        public ServiceRequestLocationDTO? ServiceRequestLocation { get; set; }
    }
}
