using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Users
{
    public class AddCreditsDTO
    {
        [Required(ErrorMessage = "Credits field is required")]
        public required int Credits { get; set; }
    }
}
