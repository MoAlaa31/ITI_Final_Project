using ITI_Project.Core.Models.Persons;
using ITI_Project.Core.Models.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Moderation
{
    public class Review : BaseEntity
    {
        public int Id{ get; set; }
        public string? Message { get; set; }
        public double Rating { get; set; }

        //Relationships
        [ForeignKey("Provider")]
        public int ProviderId { get; set; }
        public Provider Provider { get; set; }

        [ForeignKey("ServiceRequest")]
        public int ServiceRequestId { get; set; }
        public ServiceRequest ServiceRequest { get; set; }

    }
}
