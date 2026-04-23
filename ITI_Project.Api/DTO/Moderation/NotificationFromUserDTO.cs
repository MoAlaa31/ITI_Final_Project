using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Moderation
{
    public class NotificationFromUserDTO
    {
        [Required(ErrorMessage = "this field is required")]
        public required string Message { get; set; }
    }
}
