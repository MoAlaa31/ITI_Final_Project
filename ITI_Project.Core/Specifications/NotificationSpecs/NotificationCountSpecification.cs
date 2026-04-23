using ITI_Project.Core.Models.Moderation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.Specifications.NotificationSpecs
{
    public class NotificationCountSpecification : BaseSpecifications<Notification>
    {
        public NotificationCountSpecification(int clientId)
            : base(n => n.ClientId == clientId)
        {

        }
    }
}
