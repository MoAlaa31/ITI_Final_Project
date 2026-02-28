using AutoMapper;
using ITI_Project.Api.DTO.Location;
using ITI_Project.Api.DTO.Moderation;
using ITI_Project.Api.DTO.Requests;
using ITI_Project.Api.DTO.Services;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Services;

namespace ITI_Project.Api.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            /****************************************** Mapping for Services ******************************************/
            CreateMap<Service, ServiceDTO>()
                .ForMember(d => d.Name, o => o.MapFrom((src, _, _, ctx) =>
                    (ctx.Items["lang"] as string)?.ToLower() == "ar" ? src.Name_ar : src.Name_en));

            /****************************************** Mapping for Provider Documents ******************************************/
            CreateMap<ProviderDocument, ProviderDocumentDto>();

            /****************************************** Mapping for Service Request ******************************************/
            CreateMap<ServiceRequest, ServiceRequestDTO>();

            /****************************************** Mapping for Service Request ******************************************/
            CreateMap<ServiceRequestLocation, ServiceRequestLocationDTO>();
        }
    }
}
