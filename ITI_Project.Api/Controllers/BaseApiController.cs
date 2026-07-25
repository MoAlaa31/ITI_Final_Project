using ITI_Project.Api.ErrorHandling;
using ITI_Project.Core.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITI_Project.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected ActionResult HandleFailure(Error error)
        {
            var statusCode = (int)error.StatusCode;

            return StatusCode(
                statusCode,
                new ApiResponse(statusCode, error.Message));
        }
    }
}
