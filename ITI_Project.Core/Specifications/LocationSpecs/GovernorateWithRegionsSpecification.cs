using ITI_Project.Core.Models.Location;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.Specifications.LocationSpecs
{
    public class GovernorateWithRegionsSpecification : BaseSpecifications<Governorate>
    {
        public GovernorateWithRegionsSpecification() : base()
        {
            Includes.Add(g => g.Regions);
        }
    }
}
