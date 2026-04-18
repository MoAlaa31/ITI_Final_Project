using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Users;
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
        public Provider Provider { get; set; } = null!;

        [ForeignKey(nameof(ServiceRequest))]
        public required int ServiceRequestId { get; set; }
        public ServiceRequest ServiceRequest { get; set; } = null!;

        [ForeignKey(nameof(Client))]
        public required int ClientId { get; set; }
        public Client Client { get; set; } = null!;

    }
}
