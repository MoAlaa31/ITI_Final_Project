using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Moderation
{
    public class ProviderDocument : BaseEntity
    {
        public int Id { get; set; }
        public required string DocumentUrl { get; set; }
        public DocumentType DocumentType { get; set; }
        public bool IsApproved { get; set; }

        [ForeignKey("Provider")]
        public required int ProviderId { get; set; }
        public required Provider Provider { get; set; }
    }
}
