using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Services.User.DTOs.ProviderDTOs
{
    public class ServiceProviderProfileUpdateDTO
    {
        public string? Bio { get; set; }
        public string? Nickname { get; set; }
        public required int GovernorateId { get; set; }
        public required int RegionId { get; set; }
        public ServiceBaseLocationDTO? BaseLocation { get; set; }
        public List<int> ServiceIds { get; set; } = new();
    }
}
