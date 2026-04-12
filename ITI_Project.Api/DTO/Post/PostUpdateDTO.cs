using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Post
{
    public class PostUpdateDTO
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Description cannot be longer than 300 characters.")]
        public string? Description { get; set; }
    }
}