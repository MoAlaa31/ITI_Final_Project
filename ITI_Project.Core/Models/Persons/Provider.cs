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

namespace ITI_Project.Core.Models.Persons
{
    public class Provider : BaseEntity
    {
        [StringLength(100, ErrorMessage = "Bio cannot be longer than 100 characters.")]
        public string? Bio { get; set; }
        public bool Isverified { get; set; }
        public double Rating { get; set; } = 0;
        public int JobsCount { get; set; } = 0;
        [StringLength(25, ErrorMessage = "Nickname cannot be longer than 25 characters.")]
        public string? Nickname { get; set; }
        public DateOnly StartedAt { get; set; }
        public VerificationStatus VerificationStatus { get; set; }

        // Relationships
        [Required(ErrorMessage = "User Id is required.")]
        [ForeignKey(nameof(User))]
        public required int UserId { get; set; }
        public required User User { get; set; }                  //to be reviewed

        [ForeignKey(nameof(BaseLocation))]
        public int? BaseLocationId { get; set; }
        public BaseLocation? BaseLocation { get; set; }

        [ForeignKey(nameof(LiveLocation))]
        public int? LiveLocationId { get; set; }
        public LiveLocation? LiveLocation { get; set; }

        [ForeignKey(nameof(Governorate))]
        public required int GovernorateId { get; set; }
        public required Governorate Governorate { get; set; }

        [ForeignKey(nameof(Region))]
        public required int RegionId { get; set; }
        public required Region Region { get; set; }
        
        [ForeignKey(nameof(ProviderContract))]
        public int? ProviderContractId { get; set; }
        public ProviderContract? ProviderContract{ get; set; }

        public ICollection<ProviderService>? ProviderServices { get; set; }
        public ICollection<ProviderDocument>? ProviderDocuments { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<RequestOffer>? RequestOffers { get; set; }

    }
}
