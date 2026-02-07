using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.Models.Posts
{
    public class CommentReaction: BaseEntity
    {
        public required int CommentId { get; set; }
        public required Comment Comment { get; set; }

        public required int UserId { get; set; }
        public required User User { get; set; }

        public required ReactionType ReactionType { get; set; }
    }
}
