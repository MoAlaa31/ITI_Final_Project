using ITI_Project.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Moderation
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "this field is required")]
        public required string Message { get; set; }
        public string Title { get; set; } = null!;
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
