using AutoMapper;
using ITI_Project.Api.DTO.Location;
using ITI_Project.Api.DTO.Moderation;
using ITI_Project.Api.DTO.Post;
using ITI_Project.Api.DTO.Posts;
using ITI_Project.Api.DTO.Requests;
using ITI_Project.Api.DTO.Services;
using ITI_Project.Api.DTO.Users;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Posts;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Services;
using ITI_Project.Core.Models.Users;

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
            /****************************************** Mapping for Provider ******************************************/
            CreateMap<Provider, ProviderDTO>();

            /****************************************** Mapping for Client ******************************************/
            CreateMap<Client, ClientDTO>()
                .ForMember(d => d.PhoneNumbers,
                    o => o.MapFrom(s => s.phoneNumbers != null
                        ? s.phoneNumbers.Select(p => p.PhoneNumber).ToList()
                        : new List<string>()));

            CreateMap<ClientUpdateDTO, Client>()
                .ForMember(d => d.phoneNumbers, o => o.Ignore())
                .ForMember(d => d.PictureUrl, o => o.Ignore());
    
            /****************************************** Mapping for Base Location ******************************************/
            CreateMap<BaseLocation, BaseLocationDTO>();

            /****************************************** Mapping for Service Request ******************************************/
            CreateMap<ServiceRequest, ServiceRequestDTO>();
            CreateMap<ServiceRequestLocation, ServiceRequestLocationDTO>()
                .ReverseMap();
            CreateMap<ServiceRequestFromUserDTO, ServiceRequest>();

            /****************************************** Mapping for Request Offer ******************************************/
            CreateMap<RequestOffer, RequestOfferDTO>();
            CreateMap<RequestOfferFromUserDTO, RequestOffer>();

            /****************************************** Mapping for Live Location ******************************************/
            CreateMap<LiveLocation, LiveLocationDTO>();

            CreateMap<BaseLocationCreateDTO, BaseLocation>();

            /****************************************** Mapping for Posts ******************************************/
            CreateMap<Post, PostDTO>()
                .ForMember(d => d.ImageUrls,
                    o => o.MapFrom(s => s.PostImages != null
                        ? s.PostImages.Select(pi => pi.ImageUrl).ToList()
                        : new List<string>()))
                .ForMember(d => d.CommentsCount,
                    o => o.MapFrom(s => s.Comments != null ? s.Comments.Count : 0))
                .ForMember(d => d.TopReactions,
                    o => o.MapFrom(s => s.Reactions == null
                        ? new List<ReactionCountDTO>()
                        : s.Reactions
                            .GroupBy(r => r.ReactionType)
                            .Select(g => new ReactionCountDTO
                            {
                                ReactionType = g.Key,
                                Count = g.Count()
                            })
                            .OrderByDescending(r => r.Count)
                            .Take(3)
                            .ToList()));

            CreateMap<PostFromUserDTO, Post>();

            /****************************************** Mapping for Comments ******************************************/
            CreateMap<Comment, CommentDTO>()
                .ForMember(d => d.Reactions, o => o.MapFrom(s =>
                    s.Reactions == null
                        ? new List<ReactionCountDTO>()
                        : s.Reactions
                            .GroupBy(r => r.ReactionType)
                            .Select(g => new ReactionCountDTO
                            {
                                ReactionType = g.Key,
                                Count = g.Count()
                            })
                            .OrderByDescending(r => r.Count)
                            .Take(3)
                            .ToList()));
            CreateMap<CommentReaction, CommentReactionDTO>();
            CreateMap<PostReaction, PostReactionDTO>();
        }
    }
}
