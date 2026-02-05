using ITI_Project.Core;
using ITI_Project.Core.IRepository;
using ITI_Project.Repository;

namespace ITI_Project.Api.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //services.AddAutoMapper(typeof(MappingProfiles));

            return services;
        }
    }
}
