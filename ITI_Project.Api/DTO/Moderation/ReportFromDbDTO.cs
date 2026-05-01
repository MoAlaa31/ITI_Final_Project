using ITI_Project.Core.Enums;

namespace ITI_Project.Api.DTO.Moderation
{
    public class ReportFromDbDTO
    {
        public int Id { get; set; }
        public string? Reason { get; set; }
        public int ReporterId { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public string? ReporterPictureUrl { get; set; }
        public ReportType ReportType { get; set; }
        public int TargetUserId { get; set; }
        public DateTime LastUpdate { get; set; }
        public int ServiceRequestId { get; set; }
        public string TargetUserName { get; set; } = string.Empty;
        public string? TargetUserPictureUrl { get; set; }
    }
}
