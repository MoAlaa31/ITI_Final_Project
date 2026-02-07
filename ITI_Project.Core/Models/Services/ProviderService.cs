using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Services
{
    public class ProviderService: BaseEntity
    {
        [ForeignKey(nameof(Provider))]
        public required int ProviderId { get; set; }
        public required Provider Provider { get; set; }

        [ForeignKey(nameof(Service))]
        public required int ServiceId { get; set; }
        public required Service Service { get; set; }
    }
}
