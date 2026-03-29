using ITI_Project.Core.Enums;

namespace ITI_Project.Api.DTO.Posts
{
    public class CommentReactionDTO
    {
        public int ClientId { get; set; }
        public ReactionType ReactionType { get; set; }
    }
}
