using System.Collections.Generic;

namespace ITI_Project.Api.DTO.Users
{
    public class ProviderApprovalDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? PictureUrl { get; set; }
        public IReadOnlyList<ProviderDocumentItemDTO> Documents { get; set; } = new List<ProviderDocumentItemDTO>();
    }

    public class ProviderDocumentItemDTO
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}