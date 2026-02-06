using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Posts
{
    public class Post: BaseEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        // Relationships
        [ForeignKey("Region")]
        public int RegionId { get; set; }
        public Region Region { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        public ICollection<Comment> Comments { get; set; }
        public ICollection<PostImage> PostImages { get; set; }
    }
}
