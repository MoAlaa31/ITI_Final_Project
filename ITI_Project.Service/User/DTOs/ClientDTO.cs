using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;
using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Services.User.DTOs
{
    public class ClientDTO
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public Gender Gender { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public DateOnly CreatedAt { get; set; }
        public string? PictureUrl { get; set; }
        public int GovernorateId { get; set; }
        public int RegionId { get; set; }
        public List<string>? PhoneNumbers { get; set; }
    }
}
