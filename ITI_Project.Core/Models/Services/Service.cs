using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.Models.Services
{
    public class Service: BaseEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public ICollection<ProviderService>? ProviderServices { get; set; }
    }
}
