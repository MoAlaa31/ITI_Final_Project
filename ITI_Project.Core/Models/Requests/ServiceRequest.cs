using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Requests
{
    public class ServiceRequest: BaseEntity
    {
        public int Id { get; set; }
        public RequestStatus RequestStatus { get; set; }
        [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters.")]
        [Required(ErrorMessage = "Description is required.")]
        public required string Description { get; set; }
        [Range(0.0, 10000.0, ErrorMessage = "Final Price must be a positive number.")]
        public decimal? FinalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PreferredTime { get; set; }

        // Relationships
        [ForeignKey("Provider")]
        public int? ProviderId { get; set; }
        public Provider? Provider{ get; set; }

        [Required(ErrorMessage = "User Id is required.")]
        [ForeignKey("User")]
        public required int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Review")]
        public int? ReviewId { get; set; }
        public Review? Review { get; set; }

        [ForeignKey("ServiceRequestLocation")]
        public int? ServiceRequestLocationId { get; set; }
        public ServiceRequestLocation? ServiceRequestLocation { get; set; }

        public ICollection<RequestOffer>? RequestOffers { get; set; }   

    }
}
