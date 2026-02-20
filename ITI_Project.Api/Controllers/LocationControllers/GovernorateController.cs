using AutoMapper;
using ITI_Project.Api.DTO.Location;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Core;
using ITI_Project.Core.IRepository;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Specifications.LocationSpecs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITI_Project.Api.Controllers.LocationControllers
{
    public class GovernorateController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;

        private readonly IMapper mapper;

        public GovernorateController(
            IUnitOfWork unitOfWork,
            IMapper mapper
            )
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        /***************************** End point to get governorate with its Regions *****************************/
        [HttpGet("GovernorateWithRegions")]
        public async Task<ActionResult<IReadOnlyList<GovernorateDTO>>> GetAllGovernorateWithRegions([FromQuery] string? lang = "ar")
        {
            if (lang.ToLower() != "ar" && lang.ToLower() != "en")
            {
                return BadRequest(new ApiResponse(StatusCodes.Status406NotAcceptable, "Invalid Language"));
            }
            var spec = new GovernorateWithRegionsSpecification();
            var governates = await unitOfWork.Repository<Governorate>().GetAllWithSpecAsync(spec, g => new GovernorateDTO
            {
                Id = g.Id,
                Name = lang.ToLower() == "ar" ? g.Name_ar : g.Name_en,
                Regions = g.Regions.Select(r => new RegionDTO
                {
                    Id = r.Id,
                    Name = lang.ToLower() == "ar" ? r.Name_ar : r.Name_en
                }).ToList()
            });
            return Ok(governates);
        }


    }
}
