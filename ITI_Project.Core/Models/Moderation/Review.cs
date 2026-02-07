using ITI_Project.Core.Models.Persons;
using ITI_Project.Core.Models.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Moderation
{
    public class Review : BaseEntity
    {
        public int Id{ get; set; }
        [StringLength(200, ErrorMessage = "Message cannot be longer than 200 characters.")]
        public string? Message { get; set; }
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
        [Required(ErrorMessage = "Rating is required.")]
        public double Rating { get; set; }

        //Relationships
        [ForeignKey(nameof(Provider))]
        public required int ProviderId { get; set; }
        public required Provider Provider { get; set; }

        [ForeignKey(nameof(ServiceRequest))]
        public required int ServiceRequestId { get; set; }
        public required ServiceRequest ServiceRequest { get; set; }

    }
}
