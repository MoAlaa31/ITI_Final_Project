using ITI_Project.Core.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ITI_Project.Api.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToProblem(
            this Result result)
        {
            if (result.IsSuccess)
                throw new InvalidOperationException(
                    "Cannot convert success result to problem");

            var problem = new ProblemDetails
            {
                Title = "Request failed",
                Detail = result.Error.Message,
                Status = (int)result.Error.StatusCode
            };

            problem.Extensions["code"] =
                result.Error.Code;

            return new ObjectResult(problem)
            {
                StatusCode = problem.Status
            };
        }
    }
}
