namespace ITI_Project.Api.DTO.Users
{
    public class BannedProvidersDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? PictureUrl { get; set; }
        public int ProviderId { get; set; }
        public DateTime StartedAt { get; set; }
    }
}
