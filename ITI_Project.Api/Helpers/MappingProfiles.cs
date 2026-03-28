using AutoMapper;
using ITI_Project.Api.DTO.Location;
using ITI_Project.Api.DTO.Moderation;
using ITI_Project.Api.DTO.Post;
using ITI_Project.Api.DTO.Posts;
using ITI_Project.Api.DTO.Requests;
using ITI_Project.Api.DTO.Services;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Posts;
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

            /****************************************** Mapping for Request Offer ******************************************/
            CreateMap<RequestOffer, RequestOfferDTO>();

            /****************************************** Mapping for Live Location ******************************************/
            CreateMap<LiveLocation, LiveLocationDTO>();

            /****************************************** Mapping for Posts ******************************************/
            CreateMap<Post, PostDTO>()
                .ForMember(d => d.ImageUrls,
                    o => o.MapFrom(s => s.PostImages != null
                        ? s.PostImages.Select(pi => pi.ImageUrl).ToList()
                        : new List<string>()));

            CreateMap<PostFromUserDTO, Post>();
        }
    }
}
