using ITI_Project.Core.Enums;

namespace ITI_Project.Api.DTO.Account
{
    public class AccountStatusDto
    {
        public IList<string> Role { get; set; } = new List<string>();
        public bool IsProvider { get; set; }
        public ProfileStatus Status { get; set; }
    }
}