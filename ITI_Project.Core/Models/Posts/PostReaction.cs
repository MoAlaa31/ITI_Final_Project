using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text;

namespace ITI_Project.Core.Models.Posts
{
    public class PostReaction : BaseEntity
    {
        [ForeignKey(nameof(Post))]
        public required int ServicePostId { get; set; }
        public required Post Post { get; set; }

        [ForeignKey(nameof(Client))]
        public required int ClientId { get; set; }
        public required Client Client { get; set; }

        public required ReactionType ReactionType { get; set; }
    }
}
