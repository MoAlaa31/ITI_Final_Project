using ITI_Project.Core.Models.Services;
using ITI_Project.Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Services.User.DTOs.ProviderDTOs
{
    public class ServiceProviderProfileData
    {
        public Provider Provider { get; set; } = null!;
        public IReadOnlyList<Service> Services { get; set; } = new List<Service>();
        public List<string>? PhoneNumbers { get; set; }
        public int CompletedJobsCount { get; set; }
    }
}
