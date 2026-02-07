using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Posts
{
    public class Post: BaseEntity
    {
        public int Id { get; set; }
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public required string Title { get; set; }
        [StringLength(300, ErrorMessage = "Description cannot be longer than 300 characters.")]
        public string? Description { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        // Relationships
        [ForeignKey("User")]
        public required int UserId { get; set; }
        public required User User { get; set; }

        [ForeignKey(nameof(Region))]
        public int? RegionId { get; set; }
        public Region? Region { get; set; }

        [ForeignKey(nameof(Governorate))]
        public required int GovernorateId { get; set; }
        public required Governorate Governorate { get; set; }

        public ICollection<Comment>? Comments { get; set; }
        public ICollection<PostImage>? PostImages { get; set; }
        public ICollection<PostReaction>? Reactions { get; set; }
    }
}
