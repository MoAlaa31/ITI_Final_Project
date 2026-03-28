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
    }
}