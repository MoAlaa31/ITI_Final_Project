using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Persons;
using ITI_Project.Core.Models.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Moderation
{
    public class Report : BaseEntity
    {
        public int Id { get; set; }
        [StringLength(200, ErrorMessage = "Reason cannot be longer than 200 characters.")]
        public string? Reason { get; set; }
        public ReportType ReportType { get; set; }
        public DateTime LastUpdate { get; set; }
        public ReportStatus Status { get; set; }

        // Relationships
        [ForeignKey(nameof(Reporter))]
        public required int ReporterId { get; set; }
        public required User Reporter { get; set; }

        [ForeignKey(nameof(TargetUser))]
        public required int TargetUserId { get; set; }
        public required User TargetUser { get; set; }

        [ForeignKey(nameof(Resolver))]
        public int? ResolverId { get; set; }
        public User? Resolver { get; set; }

        [ForeignKey(nameof(ServiceRequest))]
        public required int ServiceRequestId { get; set; }
        public required ServiceRequest ServiceRequest { get; set; }
    }
}
