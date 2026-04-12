using ITI_Project.Core.Enums;

namespace ITI_Project.Api.DTO.Posts
{
    public class PostDTO
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int GovernorateId { get; set; }
        public int? RegionId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public IReadOnlyList<string>? ImageUrls { get; set; }
        public int CommentsCount { get; set; }
        public IReadOnlyList<ReactionCountDTO> TopReactions { get; set; } = new List<ReactionCountDTO>();
        public bool IsProvider { get; set; }
        public int? ProviderId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientPictureUrl { get; set; }
        public ReactionType? UserReaction { get; set; }
    }
}