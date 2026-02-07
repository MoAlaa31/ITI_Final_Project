using ITI_Project.Api.ErrorHandling;
using System.Net;
using System.Text.Json;

namespace ITI_Project.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionMiddleware> logger;
        private readonly IHostEnvironment hostEnvironment;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment hostEnvironment)
        {
            this.next = next;
            this.logger = logger;
            this.hostEnvironment = hostEnvironment;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next.Invoke(httpContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = hostEnvironment.IsDevelopment()
                    ? new ApiExceptionResponse((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace)
                    : new ApiExceptionResponse((int)HttpStatusCode.InternalServerError);

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                var json = JsonSerializer.Serialize(response, options);

                await httpContext.Response.WriteAsync(json);
            }
        }
    }
}
