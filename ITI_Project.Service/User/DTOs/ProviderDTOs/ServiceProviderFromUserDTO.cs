namespace ITI_Project.Services.User.DTOs.ProviderDTOs
{
    public class ServiceProviderFromUserDTO
    {
        public string? Bio { get; set; }
        public string? Nickname { get; set; }
        public required int GovernorateId { get; set; }
        public required int RegionId { get; set; }
    }
}
