using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.Specifications.NotificationSpecs
{
    public class NotificationWithPaginationSpecification : BaseSpecifications<Notification>
    {
        public NotificationWithPaginationSpecification(int clientId, NotificationSpecParams specParams)
            : base(n => n.ClientId == clientId)
        {

            AddOrderByDescending(n => n.CreatedAt);

            // Pagination
            ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
        }
    }
}
