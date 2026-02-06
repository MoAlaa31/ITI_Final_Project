using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Moderation
{
    public class ProviderDocument: BaseEntity
    {
        public int Id { get; set; }
        public string DocumentUrl { get; set; }
        public DocumentType DocumentType { get; set; }
        public string FileUrl { get; set; }
        public bool IsApproved { get; set; }

        [ForeignKey("Provider")]
        public int ProviderId { get; set; }
        public Provider Provider { get; set; }
    }
}
