using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Posts
{
    public class CommentUpdateDTO
    {
        [Required(ErrorMessage = "Message is required.")]
        [StringLength(300, ErrorMessage = "Message cannot be longer than 300 characters.")]
        public string Message { get; set; } = string.Empty;
    }
}