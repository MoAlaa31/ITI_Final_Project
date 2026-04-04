using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Moderation
{
    public class ProviderDocumentUpdateDTO
    {
        [Required(ErrorMessage = "Document file is required")]
        public IFormFile DocumentFile { get; set; } = null!;
        public string? FileName { get; set; }
    }
}
