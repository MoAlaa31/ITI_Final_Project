using AutoMapper;
using ITI_Project.Api.DTO.Services;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Filters;
using ITI_Project.Core;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Persons;
using ITI_Project.Core.Models.Services;
using ITI_Project.Core.Specifications.ServiceSpecs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace ITI_Project.Api.Controllers.ServicesControllers
{
    public class ProviderServicesController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ProviderServicesController(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpGet("provider/{providerId:int}")]
        [ServiceFilter(typeof(ExistingIdFilter<Provider>))]
        public async Task<ActionResult<IReadOnlyList<ServiceDTO>>> GetProviderServices(int providerId, [FromQuery] string? lang = "ar")
        {
            if (lang?.ToLower() != "ar" && lang?.ToLower() != "en")
                return BadRequest(new ApiResponse(StatusCodes.Status406NotAcceptable, "Invalid Language"));

            var spec = new ProviderServicesByProviderIdSpecification(providerId);
            var providerServices = await unitOfWork.Repository<ProviderService>().GetAllWithSpecAsync(spec);

            if(providerServices == null || providerServices.Count == 0)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "No services found for this provider"));
            //var result = providerServices.Select(ps => new ServiceDTO
            //{
            //    Id = ps.ServiceId,
            //    Name = lang?.ToLower() == "ar" ? ps.Service.Name_ar : ps.Service.Name_en
            //}).ToList();
            var dto = mapper.Map<IReadOnlyList<ServiceDTO>>(providerServices?.Select(ps => ps.Service).ToList(), opt => opt.Items["lang"] = lang);

            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpPut("provider/{providerId:int}")]
        [ServiceFilter(typeof(ExistingIdFilter<Provider>))]
        public async Task<ActionResult> UpdateProviderServices(int providerId, [FromBody] UpdateProviderServicesDto dto)
        {
            if (dto?.ServiceIds == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "ServiceIds are required"));

            var distinctIds = dto.ServiceIds.Distinct().ToList();
            var services = await unitOfWork.Repository<Service>().GetManyByConditionAsync(s => distinctIds.Contains(s.Id)) ?? new List<Service>();
            if (services.Count != distinctIds.Count)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "One or more ServiceIds are invalid"));

            var existing = await unitOfWork.Repository<ProviderService>().GetManyByConditionAsync(ps => ps.ProviderId == providerId) ?? new List<ProviderService>();

            var existingIds = existing.Select(x => x.ServiceId).ToHashSet();
            var newIds = distinctIds.ToHashSet();

            var toRemove = existing.Where(ps => !newIds.Contains(ps.ServiceId)).ToList();
            var toAdd = newIds.Where(id => !existingIds.Contains(id))
                              .Select(id => new ProviderService { ProviderId = providerId, ServiceId = id })
                              .ToList();

            if (toRemove.Count > 0)
                unitOfWork.Repository<ProviderService>().DeleteRange(toRemove);

            if (toAdd.Count > 0)
                await unitOfWork.Repository<ProviderService>().AddRangeAsync(toAdd);

            await unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Provider services updated"));
        }
    }
}
