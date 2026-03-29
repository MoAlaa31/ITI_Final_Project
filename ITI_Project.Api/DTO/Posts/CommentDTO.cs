using System;
using System.Collections.Generic;

namespace ITI_Project.Api.DTO.Posts
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int ClientId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public IReadOnlyList<ReactionCountDTO> Reactions { get; set; } = new List<ReactionCountDTO>();
    }
}
