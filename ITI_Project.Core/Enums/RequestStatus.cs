using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.Enums
{
    public enum RequestStatus
    {
        Open,        // created, waiting for offers or assignment
        Assigned,    // provider selected
        InProgress,  // provider started work
        Completed,   // work finished
        Cancelled    // cancelled by user or admin
    }

}
