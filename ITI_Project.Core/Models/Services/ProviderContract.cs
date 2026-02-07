using ITI_Project.Core.Models.Persons;
using ITI_Project.Core.Models.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Services
{
    public class ProviderContract: BaseEntity
    {
        [ForeignKey(nameof(Provider))]
        public required int ProviderId { get; set; }
        public required Provider Provider { get; set; }

        [ForeignKey(nameof(ServiceRequest))]
        public required int ServiceRequestId { get; set; }
        public required ServiceRequest ServiceRequest { get; set; }
    }
}
