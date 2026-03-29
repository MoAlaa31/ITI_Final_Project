using System.Text.Json.Serialization;

namespace ITI_Project.Api.DTO.Account
{
    public class ClientDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PictureUrl { get; set; }
        public string AccessToken { get; set; }
        public IList<string> Role { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsProvider { get; set; }
        public string? ProviderStatus { get; set; }
        [JsonIgnore]
        public DateTime AccessTokenExpiration { get; set; }
    }
}
