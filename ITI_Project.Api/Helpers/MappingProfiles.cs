using AutoMapper;
using ITI_Project.Api.DTO.Services;
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
        }
    }
}
