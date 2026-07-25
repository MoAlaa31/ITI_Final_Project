namespace ITI_Project.Api.ErrorHandling
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }

        public ApiResponse(int statusCode , string? message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }

        private static string? GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request: The server could not understand the request.",
                401 => "Unauthorized: Authentication is required.",
                403 => "Forbidden: You do not have permission to access this resource.",
                404 => "Not Found: The requested resource could not be found.",
                409 => "Conflict: The request conflicts with the current state.",
                500 => "Internal Server Error: An unexpected error occurred.",
                _ => null
            };
        }
    }
}
