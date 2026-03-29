using ITI_Project.Core.Models.Users;
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
        [ForeignKey("Client")]
        public required int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        [ForeignKey("Post")]
        public required int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public ICollection<CommentReaction>? Reactions { get; set; }

    }
}
