using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Users
{
    public class ProviderFromUserDTO
    {
        [StringLength(100, ErrorMessage = "Bio cannot be longer than 100 characters.")]
        public string? Bio { get; set; }
        [StringLength(25, ErrorMessage = "Nickname cannot be longer than 25 characters.")]
        public string? Nickname { get; set; }
        public required int GovernorateId { get; set; }
        public required int RegionId { get; set; }
    }
}
