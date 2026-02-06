using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Requests
{
    public class RequestOffer: BaseEntity
    {
        public int Id { get; set; }
        [Range(0.0, 10000.0, ErrorMessage = "Price must be a positive number.")]
        decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        [StringLength(200, ErrorMessage = "Message cannot be longer than 200 characters.")]
        [Required(ErrorMessage = "Message is required.")]
        public required string Message { get; set; }
        public RequestStatus Status { get; set; }

        // Relationships
        [ForeignKey("ServiceRequest")]
        public int ServiceRequestId { get; set; }
        public ServiceRequest ServiceRequest { get; set; }

        [ForeignKey("Provider")]
        public int ProviderId { get; set; }
        public Provider Provider { get; set; }
    }
}
