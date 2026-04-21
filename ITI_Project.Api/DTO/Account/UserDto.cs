using System.Text.Json.Serialization;
using ITI_Project.Core.Enums;

namespace ITI_Project.Api.DTO.Account
{
    public class ClientDto
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public string? PictureUrl { get; set; }
        public required string AccessToken { get; set; }
        public IList<string> Role { get; set; } = new List<string>();
        public bool IsAuthenticated { get; set; }
        public bool IsProvider { get; set; }
        public ProfileStatus Status { get; set; }
        [JsonIgnore]
        public DateTime AccessTokenExpiration { get; set; }
    }
}
