using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Specifications.ServiceRequestSpecs;

namespace ITI_Project.Core.Specifications.RequestSpecs
{
    public class ProviderServiceRequestWithPaginationSpecification : BaseSpecifications<ServiceRequest>
    {
        public ProviderServiceRequestWithPaginationSpecification(RequestSpecParams specParams, IReadOnlyCollection<int> serviceIds)
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
                && serviceIds.Contains(sr.ServiceId)
            )
        {
            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch(specParams.Sort.ToLower())
                {
                    case "createdatasc":
                        AddOrderBy(sr => sr.CreatedAt);
                        break;
                    case "createdatdesc":
                        AddOrderByDescending(sr => sr.CreatedAt);
                        break;
                    case "priceasc":
                        AddOrderBy(sr => sr.FinalPrice);
                        break;
                    case "pricedesc":
                        AddOrderByDescending(sr => sr.FinalPrice);
                        break;
                    default:
                        AddOrderByDescending(sr => sr.CreatedAt);
                        break;
                }
            }
            else
            {
                AddOrderByDescending(sr => sr.CreatedAt);
            }

            Includes.Add(sr => sr.ServiceRequestLocation);
            ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
        }
    }
}
