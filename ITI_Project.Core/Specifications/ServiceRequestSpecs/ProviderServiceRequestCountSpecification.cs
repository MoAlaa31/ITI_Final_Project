using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Specifications.ServiceRequestSpecs;

namespace ITI_Project.Core.Specifications.RequestSpecs
{
    public class ProviderServiceRequestCountSpecification : BaseSpecifications<ServiceRequest>
    {
        public ProviderServiceRequestCountSpecification(RequestSpecParams specParams, IReadOnlyCollection<int> serviceIds)
            : base(sr =>
                sr.ProviderId == null
                && sr.RequestStatus == Enums.RequestStatus.Open
                && sr.ServiceRequestLocation != null
                && (!specParams.MinLatitude.HasValue || sr.ServiceRequestLocation.Latitude >= specParams.MinLatitude)
                && (!specParams.MaxLatitude.HasValue || sr.ServiceRequestLocation.Latitude <= specParams.MaxLatitude)
                && (!specParams.MinLongitude.HasValue || sr.ServiceRequestLocation.Longitude >= specParams.MinLongitude)
                && (!specParams.MaxLongitude.HasValue || sr.ServiceRequestLocation.Longitude <= specParams.MaxLongitude)
                && (string.IsNullOrEmpty(specParams.Search) || sr.Description.ToLower().Contains(specParams.Search))
                && (!specParams.MinPrice.HasValue || sr.FinalPrice >= specParams.MinPrice)
                && (sr.ServiceId.HasValue && serviceIds.Contains(sr.ServiceId.Value))
            )
        {
        }
    }
}