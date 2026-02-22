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
        public string? Bio { get; set; }
        public bool Isverified { get; set; }
        public double? Rating { get; set; }
        public int JobsCount { get; set; }
        [StringLength(25, ErrorMessage = "Nickname cannot be longer than 25 characters.")]
        public string? Nickname { get; set; }
        public DateOnly StartedAt { get; set; }
        public VerificationStatus VerificationStatus { get; set; }

        // Relationships
        [Required(ErrorMessage = "User Id is required.")]
        [ForeignKey(nameof(Client))]
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public BaseLocation? BaseLocation { get; set; }
        public LiveLocation? LiveLocation { get; set; }

        [ForeignKey(nameof(Governorate))]
        public int GovernorateId { get; set; }
        public Governorate Governorate { get; set; } = null!;

        [ForeignKey(nameof(Region))]
        public int RegionId { get; set; }
        public Region Region { get; set; } = null!;

        public ICollection<ProviderService>? ProviderServices { get; set; }
        public ICollection<ProviderDocument>? ProviderDocuments { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<RequestOffer>? RequestOffers { get; set; }
        public ICollection<ServiceRequest>? ServiceRequests { get; set; }

    }
}
