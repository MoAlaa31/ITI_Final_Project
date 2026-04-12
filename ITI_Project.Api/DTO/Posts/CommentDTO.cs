using System;
using System.Collections.Generic;
using ITI_Project.Core.Enums;

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
        public string ClientName { get; set; } = string.Empty;
        public string? ClientPictureUrl { get; set; }
        public bool IsProvider { get; set; }
        public int? ProviderId { get; set; }
        public ReactionType? UserReaction { get; set; }
    }
}
