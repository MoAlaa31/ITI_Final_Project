using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Core.Enums
{
    public enum AdminActionType
    {
        ApproveProvider,
        RejectProvider,
        SuspendProvider,
        BanUser,
        UnbanUser,
        ResolveReport,
        DeletePost,
        RestorePost
    }
}
