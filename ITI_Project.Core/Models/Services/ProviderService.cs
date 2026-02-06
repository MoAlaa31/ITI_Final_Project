using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Services
{
    public class ProviderService: BaseEntity
    {
        [ForeignKey("Provider")]
        public int ProviderId { get; set; }
        public Provider Provider { get; set; }

        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public Service Service { get; set; }
    }
}
