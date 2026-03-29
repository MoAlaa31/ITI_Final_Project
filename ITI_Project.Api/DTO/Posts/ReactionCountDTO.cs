using ITI_Project.Core.Enums;

namespace ITI_Project.Api.DTO.Posts
{
    public class ReactionCountDTO
    {
        public ReactionType ReactionType { get; set; }
        public int Count { get; set; }
    }
}
