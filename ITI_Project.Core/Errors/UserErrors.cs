using System.Net;

namespace ITI_Project.Core.Errors
{
    public static class UserErrors
    {
        public static Error NotFound =
            new(
                "User.NotFound",
                "User not found",
                HttpStatusCode.NotFound);

        public static Error ActiveReports =
            new(
                "User.ActiveReports",
                "Cannot delete account while active reports exist",
                HttpStatusCode.BadRequest);
    }
}
