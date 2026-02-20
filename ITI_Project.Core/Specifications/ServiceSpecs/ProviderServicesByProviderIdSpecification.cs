using ITI_Project.Core.Models.Services;
using ITI_Project.Core.Specifications;

namespace ITI_Project.Core.Specifications.ServiceSpecs
{
    public class ProviderServicesByProviderIdSpecification : BaseSpecifications<ProviderService>
    {
        public ProviderServicesByProviderIdSpecification(int providerId)
            : base(ps => ps.ProviderId == providerId)
        {
            Includes.Add(ps => ps.Service);
        }
    }
}
