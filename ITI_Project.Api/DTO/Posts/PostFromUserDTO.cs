using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Posts
{
    public class PostFromUserDTO
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; } = null!;
        [StringLength(300, ErrorMessage = "Description cannot be longer than 300 characters.")]
        public string? Description { get; set; }

        public List<IFormFile>? Images { get; set; }
    }
}
