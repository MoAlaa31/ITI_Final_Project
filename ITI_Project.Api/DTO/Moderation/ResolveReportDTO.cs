using ITI_Project.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Moderation
{
    public class ResolveReportDTO
    {
        [Required(ErrorMessage = "Status is Required")]
        public required ReportStatus Status { get; set; }

        [Required(ErrorMessage = "AdminNote is Required")]
        public required string? AdminNote { get; set; }
    }
}
