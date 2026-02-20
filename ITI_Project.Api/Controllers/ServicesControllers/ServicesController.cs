using ITI_Project.Api.DTO.Services;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Core;
using ITI_Project.Core.Models.Services;
using ITI_Project.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITI_Project.Api.Controllers.ServicesControllers
{
    public class ServicesController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;

        public ServicesController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("services")]
        public async Task<ActionResult<IReadOnlyList<ServiceDTO>>> GetServices([FromQuery] string? lang = "ar")
        {
            if (lang?.ToLower() != "ar" && lang?.ToLower() != "en")
                return BadRequest(new ApiResponse(StatusCodes.Status406NotAcceptable, "Invalid Language"));

            var services = await unitOfWork.Repository<Service>().GetAllAsync();
            var result = services.Select(s => new ServiceDTO
            {
                Id = s.Id,
                Name = lang?.ToLower() == "ar" ? s.Name_ar : s.Name_en
            }).ToList();

            return Ok(result);
        }
    }
}
