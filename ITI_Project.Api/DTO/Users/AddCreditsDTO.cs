using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Users
{
    public class AddCreditsDTO
    {
        [Required(ErrorMessage = "Amount field is required")]
        public required int Amount { get; set; }
    }
}
