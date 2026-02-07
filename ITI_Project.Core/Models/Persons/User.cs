using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Posts;
using ITI_Project.Core.Models.Requests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ITI_Project.Core.Models.Persons
{
    public class User : BaseEntity
    {
        public int Id { get; set; }
        public required string AppUserId { get; set; }
        [StringLength(25, ErrorMessage = "First name cannot be longer than 25 characters.")]
        [Required(ErrorMessage = "First Name is required.")]
        public required string FirstName { get; set; }
        [StringLength(25, ErrorMessage = "Last name cannot be longer than 25 characters.")]
        [Required(ErrorMessage = "Last Name is required.")]
        public required string LastName { get; set; }
        [Required(ErrorMessage = "Gender is required.")]
        public Gender Gender { get; set; }
        [Required(ErrorMessage = "Date of birth is required.")]
        public DateOnly DateOfBirth { get; set; }
        public DateOnly CreatedAt { get; set; }
        public string? PictureUrl { get; set; }

        // Relationships
        public Provider? Provider { get; set; }

        public ICollection<ServiceRequest>? ServiceRequests { get; set; }
        public ICollection<Post>? ServicePosts { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<UserPhoneNumber>? phoneNumbers { get; set; }
        public ICollection<AdminActionLog>? AdminActionLogs { get; set; }
        public ICollection<PostReaction> PostReactions { get; set; }
        public ICollection<CommentReaction> CommentReactions { get; set; }


    }
}
