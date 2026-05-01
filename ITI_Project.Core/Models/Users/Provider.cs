using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Users
{
    public class Provider : BaseEntity
    {
        public int Id { get; set; }
        [StringLength(100, ErrorMessage = "Bio cannot be longer than 100 characters.")]
        [MaxLength(100)]
        public string? Bio { get; set; }
        public bool Isverified { get; set; }
        public double? Rating { get; set; }
        public double RatingSum { get; set; }
        public int ReviewsCount { get; set; }
        public int Credits { get; set; } = 0;
        public int JobsCount { get; set; }
        [StringLength(25, ErrorMessage = "Nickname cannot be longer than 25 characters.")]
        [MaxLength(25)]
        public string? Nickname { get; set; }
        public DateTime StartedAt { get; set; }
        public VerificationStatus VerificationStatus { get; set; }

        // Relationships
        [Required(ErrorMessage = "User Id is required.")]
        [ForeignKey(nameof(Client))]
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public BaseLocation? BaseLocation { get; set; }
        public LiveLocation? LiveLocation { get; set; }

        public ICollection<ProviderService>? ProviderServices { get; set; }
        public ICollection<ProviderDocument>? ProviderDocuments { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<RequestOffer>? RequestOffers { get; set; }
        public ICollection<ServiceRequest>? ServiceRequests { get; set; }

    }
}
