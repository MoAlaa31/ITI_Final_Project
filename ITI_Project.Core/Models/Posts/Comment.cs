using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Posts
{
    public class Comment : BaseEntity
    {
        public int Id { get; set; }
        [StringLength(300, ErrorMessage = "Message cannot be longer than 300 characters.")]
        public required string Message { get; set; }
        public DateTime CreatedAt { get; set; }

        //Relationships
        [ForeignKey("User")]
        public required int UserId { get; set; }
        public required User User { get; set; }

        [ForeignKey("Post")]
        public required int PostId { get; set; }
        public required Post Post { get; set; }

        public ICollection<CommentReaction>? Reactions { get; set; }

    }
}
