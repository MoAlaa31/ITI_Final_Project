using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.Models.Services
{
    public class Service: BaseEntity
    {
        public int Id { get; set; }
        public required string Name_ar { get; set; }
        public required string Name_en { get; set; }

        public ICollection<ProviderService>? ProviderServices { get; set; }
    }
}
