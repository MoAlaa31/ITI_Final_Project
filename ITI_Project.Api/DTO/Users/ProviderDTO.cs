using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITI_Project.Api.DTO.Users
{
    public class ProviderDTO
    {
        [StringLength(100, ErrorMessage = "Bio cannot be longer than 100 characters.")]
        public string? Bio { get; set; }
        [StringLength(25, ErrorMessage = "Nickname cannot be longer than 25 characters.")]
        public string? Nickname { get; set; }
        public required int GovernorateId { get; set; }
        public required int RegionId { get; set; }
    }
}
