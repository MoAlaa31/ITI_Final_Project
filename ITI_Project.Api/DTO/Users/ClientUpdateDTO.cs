using ITI_Project.Core.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Users
{
    public class ClientUpdateDTO
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(25, ErrorMessage = "First name cannot be longer than 25 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(25, ErrorMessage = "Last name cannot be longer than 25 characters.")]
        public string LastName { get; set; } = string.Empty;
        public Gender? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public IFormFile? Picture { get; set; }
        public List<string>? PhoneNumbers { get; set; }
    }
}
