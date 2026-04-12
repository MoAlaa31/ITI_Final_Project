using ITI_Project.Core.Enums;

namespace ITI_Project.Api.DTO.Posts
{
    public class CommentReactionDetailsDTO
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientPictureUrl { get; set; }
        public ReactionType ReactionType { get; set; }
    }
}