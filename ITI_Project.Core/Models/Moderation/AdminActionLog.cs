using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Moderation
{
    public class AdminActionLog : BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        [StringLength(200, ErrorMessage = "Last name cannot be longer than 200 characters.")]
        public string? Description { get; set; }
        public AdminActionType ActionType { get; set; }
        public TargetType TargetType { get; set; }
        public int TargetId { get; set; }


        // Relationships
        [ForeignKey(nameof(Admin))]
        public required int AdminId { get; set; }
        public required User Admin { get; set; }
    }
}
