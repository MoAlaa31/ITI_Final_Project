using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Posts
{
    public class CommentCreateDTO
    {
        [Required(ErrorMessage = "PostId is required.")]
        public int PostId { get; set; }

        [Required(ErrorMessage = "Message is required.")]
        [StringLength(300, ErrorMessage = "Message cannot be longer than 300 characters.")]
        public string Message { get; set; } = string.Empty;
    }
}
