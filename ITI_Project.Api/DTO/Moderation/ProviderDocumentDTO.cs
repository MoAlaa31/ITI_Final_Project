using ITI_Project.Core.Enums;

namespace ITI_Project.Api.DTO.Moderation
{
    public class ProviderDocumentDto
    {
        public int Id { get; set; }
        public string DocumentUrl { get; set; } = string.Empty;
        public DocumentType DocumentType { get; set; }
        public bool IsApproved { get; set; }
        public int ProviderId { get; set; }
    }
}