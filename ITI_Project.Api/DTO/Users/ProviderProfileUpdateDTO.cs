using ITI_Project.Api.DTO.Location;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Users
{
    public class ProviderProfileUpdateDTO
    {
        [StringLength(100, ErrorMessage = "Bio cannot be longer than 100 characters.")]
        public string? Bio { get; set; }

        [StringLength(25, ErrorMessage = "Nickname cannot be longer than 25 characters.")]
        public string? Nickname { get; set; }

        [Required(ErrorMessage = "GovernorateId is required.")]
        public int GovernorateId { get; set; }

        [Required(ErrorMessage = "RegionId is required.")]
        public int RegionId { get; set; }

        public BaseLocationCreateDTO? BaseLocation { get; set; }

        public List<int> ServiceIds { get; set; } = new();
    }
}
