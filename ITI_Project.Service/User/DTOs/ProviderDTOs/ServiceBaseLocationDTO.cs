using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Services.User.DTOs.ProviderDTOs
{
    public class ServiceBaseLocationDTO
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? AddressText { get; set; }
    }
}
