using AutoMapper;
using ITI_Project.Api.DTO.Credit;
using ITI_Project.Api.DTO.Location;
using ITI_Project.Api.DTO.Moderation;
using ITI_Project.Api.DTO.Posts;
using ITI_Project.Api.DTO.Requests;
using ITI_Project.Api.DTO.Services;
using ITI_Project.Api.DTO.Users;
using ITI_Project.Core.Models.Credit;
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

            CreateMap<Provider, ProviderProfileDTO>()
                .ForMember(d => d.Name, o => o.MapFrom(s => $"{s.Client.FirstName} {s.Client.LastName}".Trim()))
                .ForMember(d => d.PictureUrl, o => o.MapFrom(s => s.Client.PictureUrl))
                .ForMember(d => d.GovernorateId, o => o.MapFrom(s => s.Client.GovernorateId))
                .ForMember(d => d.RegionId, o => o.MapFrom(s => s.Client.RegionId))
                .ForMember(d => d.Services, o => o.Ignore());

            CreateMap<Provider, ProviderProfilePrivateDTO>()
                .IncludeBase<Provider, ProviderProfileDTO>();
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
            CreateMap<ServiceRequest, ServiceRequestDTO>()
                .ForMember(d => d.ImageUrls,
                    o => o.MapFrom(s => s.ServiceRequestImages != null
                        ? s.ServiceRequestImages.Select(i => i.ImageUrl).ToList()
                        : new List<string>()));

            CreateMap<ServiceRequest, ServiceRequestByIdDTO>()
                .IncludeBase<ServiceRequest, ServiceRequestDTO>();

            CreateMap<ServiceRequest, ServiceRequestToClientDTO>()
                .ForMember(d => d.IsReported,
                    o => o.MapFrom(s => s.Reports != null && s.Reports.Any()))
                .IncludeBase<ServiceRequest, ServiceRequestDTO>();

            CreateMap<ServiceRequest, ServiceRequestProviderDTO>()
                .ForMember(d => d.ImageUrls,
                    o => o.MapFrom(s => s.ServiceRequestImages != null
                        ? s.ServiceRequestImages.Select(i => i.ImageUrl).ToList()
                        : new List<string>()))
                .ForMember(d => d.ClientName, o => o.MapFrom(s => s.Client != null ? $"{s.Client.FirstName} {s.Client.LastName}" : string.Empty))
                .ForMember(d => d.ClientPictureUrl, o => o.MapFrom(s => s.Client != null ? s.Client.PictureUrl : null));

            CreateMap<ServiceRequestLocation, ServiceRequestLocationDTO>()
                .ReverseMap();
            CreateMap<ServiceRequestFromUserDTO, ServiceRequest>();

            CreateMap<ServiceRequest, AvailableServiceRequestDTO>()
                .IncludeBase<ServiceRequest, ServiceRequestProviderDTO>();

            /****************************************** Mapping for Request Offer ******************************************/
            CreateMap<RequestOffer, RequestOfferDTO>();
            CreateMap<RequestOffer, RequestOfferProviderDTO>();
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
                            .ToList()))
                .ForMember(d => d.IsProvider, o => o.MapFrom(s => s.Client!.Provider != null))
                .ForMember(d => d.ProviderId, o => o.MapFrom(s => s.Client!.Provider != null ? s.Client.Provider.Id : (int?)null))
                .ForMember(d => d.ClientName, o => o.MapFrom(s => s.Client != null ? $"{s.Client.FirstName} {s.Client.LastName}" : string.Empty))
                .ForMember(d => d.ClientPictureUrl, o => o.MapFrom(s => s.Client != null ? s.Client.PictureUrl : null));

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
                            .ToList()))
                .ForMember(d => d.ClientName, o => o.MapFrom(s => s.Client != null ? $"{s.Client.FirstName} {s.Client.LastName}" : string.Empty))
                .ForMember(d => d.ClientPictureUrl, o => o.MapFrom(s => s.Client != null ? s.Client.PictureUrl : null))
                .ForMember(d => d.IsProvider, o => o.MapFrom(s => s.Client!.Provider != null))
                .ForMember(d => d.ProviderId, o => o.MapFrom(s => s.Client!.Provider != null ? s.Client.Provider.Id : (int?)null));

            CreateMap<Comment, CommentCreateResultDTO>();
            CreateMap<PostReaction, PostReactionDTO>();

            /****************************************** Mapping for Comments ******************************************/
            CreateMap<Notification, NotificationDTO>();

            /****************************************** Mapping for Reviews ******************************************/
            CreateMap<Review, ReviewDto>()
                .ForMember(d => d.ClientName,
                    o => o.MapFrom(s =>
                        s.Client != null
                            ? (s.Client.FirstName + " " + s.Client.LastName).Trim()
                            : string.Empty))
                .ForMember(d => d.ClientPictureUrl,
                    o => o.MapFrom(s => s.Client != null ? s.Client.PictureUrl : null));

            /****************************************** Mapping for Report ******************************************/
            CreateMap<Report, ReportFromDbDTO>()
                .ForMember(d => d.ReporterName, o => o.MapFrom(s =>
                    s.Reporter != null ? $"{s.Reporter.FirstName} {s.Reporter.LastName}" : string.Empty))
                .ForMember(d => d.ReporterPictureUrl, o => o.MapFrom(s =>
                    s.Reporter != null ? s.Reporter.PictureUrl : null))
                .ForMember(d => d.TargetUserName, o => o.MapFrom(s =>
                    s.TargetUser != null ? $"{s.TargetUser.FirstName} {s.TargetUser.LastName}" : string.Empty))
                .ForMember(d => d.TargetUserPictureUrl, o => o.MapFrom(s =>
                    s.TargetUser != null ? s.TargetUser.PictureUrl : null));

            CreateMap<Provider, BannedProvidersDTO>()
                .ForMember(d => d.Name, o => o.MapFrom(s =>
                    s.Client != null
                        ? $"{s.Client.FirstName} {s.Client.LastName}".Trim()
                        : string.Empty))
                .ForMember(d => d.PictureUrl, o => o.MapFrom(s => s.Client != null ? s.Client.PictureUrl : null))
                .ForMember(d => d.ProviderId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.StartedAt, o => o.MapFrom(s => s.StartedAt));

            /****************************************** Mapping for Payments ******************************************/
            CreateMap<Payment, PaymentDTO>();

            /****************************************** Mapping for Credit Transactions ******************************************/
            CreateMap<CreditTransaction, CreditTransactionDTO>();
        }
    }
}
