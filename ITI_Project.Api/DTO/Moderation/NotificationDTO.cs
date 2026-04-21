using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Moderation
{
    public class NotificationDTO
    {
        [Required(ErrorMessage = "this field is required")]
        public required string message { get; set; }
    }
}
