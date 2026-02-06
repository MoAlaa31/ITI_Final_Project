using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Moderation
{
    public class AdminActionLog : BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public AdminActionType ActionType { get; set; }
        public TargetType TargetType { get; set; }


        // Relationships
        [ForeignKey("Target")]
        public int TargetId { get; set; }
        public User Target { get; set; }

        [ForeignKey("Admin")]
        public int AdminId { get; set; }
        public User Admin { get; set; }
    }
}
