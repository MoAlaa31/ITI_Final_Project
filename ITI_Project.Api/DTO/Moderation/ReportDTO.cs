using ITI_Project.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Moderation
{
    public class ReportDTO
    {
        [Required(ErrorMessage = "ServiceRequestId is Required")]
        public int ServiceRequestId { get; set; }
        public string? Reason { get; set; }
        [Required(ErrorMessage = "ReportType is Required")]
        public ReportType ReportType { get; set; }
    }
}
