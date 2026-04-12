using ITI_Project.Api.ErrorHandling;
using ITI_Project.Core;
using ITI_Project.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace ITI_Project.Api.Filters
{
    public class ExistingIdFilter<T> : IAsyncActionFilter where T : BaseEntity
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExistingIdFilter(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var idArgument = context.ActionArguments
                .FirstOrDefault(a => a.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase));

            if (idArgument.Value is int id && id > 0)
            {
                var entity = await _unitOfWork.Repository<T>().GetAsync(id);
                if (entity == null)
                {
                    context.Result = new NotFoundObjectResult(new ApiResponse(StatusCodes.Status404NotFound, $"{typeof(T).Name} with ID {id} not found."));
                    return;
                }
            }
            else
            {
                context.Result = new BadRequestObjectResult(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid ID format."));
                return;
            }

            await next();
        }
    }
}
