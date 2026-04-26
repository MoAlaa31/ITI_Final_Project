using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Posts
{
    public class CommentReaction: BaseEntity
    {
        [ForeignKey(nameof(Comment))]
        public required int CommentId { get; set; }
        public Comment Comment { get; set; } = null!;

        [ForeignKey(nameof(Client))]
        public int? ClientId { get; set; }
        public Client? Client { get; set; }

        public required ReactionType ReactionType { get; set; }
    }
}
