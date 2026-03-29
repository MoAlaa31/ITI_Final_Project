using ITI_Project.Api.DTO.Users;
using ITI_Project.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Account
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "FirstName is required.")]
        [StringLength(maximumLength: 50, MinimumLength = 2, ErrorMessage = "FirstName must be at least 2 characters and at most 50 characters.")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required.")]
        [StringLength(maximumLength: 50, MinimumLength = 2, ErrorMessage = "LastName must be at least 2 characters and at most 50 characters.")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [RegularExpression("^(?!\\d)[a-zA-Z0-9._%+-]{3,}@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$",
        ErrorMessage = "Email must not start with a number and must have at least 3 characters before '@'")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,15}$",
            ErrorMessage = "Password must be between 8 and 15 characters and contain at least one lowercase letter," +
            " one uppercase letter, one digit and one special character.")]
        public required string Password { get; set; }
        public bool IsProvider { get; set; } = false;
    }
}
