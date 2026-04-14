using ITI_Project.Api.DTO.Location;
using ITI_Project.Api.DTO.Services;

namespace ITI_Project.Api.DTO.Users
{
    public class ProviderProfileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PictureUrl { get; set; }
        public string? Bio { get; set; }
        public string? Nickname { get; set; }
        public double? Rating { get; set; }
        public int ReviewsCount { get; set; }
        public int JobsCount { get; set; }
        public int? GovernorateId { get; set; }
        public int? RegionId { get; set; }
        public BaseLocationDTO? BaseLocation { get; set; }
        public IReadOnlyList<ServiceDTO> Services { get; set; } = new List<ServiceDTO>();
        public List<string>? PhoneNumbers { get; set; }
    }
}