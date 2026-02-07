using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ITI_Project.Core.Models.Posts
{
    public class PostReaction : BaseEntity
    {
        public required int ServicePostId { get; set; }
        public required Post Post { get; set; }

        public required int UserId { get; set; }
        public required User User { get; set; }

        public required ReactionType ReactionType { get; set; }
    }
}
