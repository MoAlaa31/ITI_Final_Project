using ITI_Project.Api.Attributes;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Persons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITI_Project.Api.DTO.Moderation
{
    public class ProviderDocumentUploadDTO
    {
        [ValidEnum<DocumentType>]
        public DocumentType DocumentType { get; set; }
        [Required(ErrorMessage = "Document file is required")]
        public IFormFile DocumentFile { get; set; }
        public string? FileName { get; set; }
    }
}
