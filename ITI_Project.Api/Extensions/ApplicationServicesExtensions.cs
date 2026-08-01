using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Filters;
using ITI_Project.Api.Helpers;
using ITI_Project.Api.Settings;
using ITI_Project.Core;
using ITI_Project.Core.IRepository;
using ITI_Project.Core.IServices;
using ITI_Project.Repository;
using ITI_Project.Services;
using ITI_Project.Services.AzureAi;
using ITI_Project.Services.BackgroundServices;
using ITI_Project.Services.Credit;
using ITI_Project.Services.Files;
using ITI_Project.Services.Location;
using ITI_Project.Services.User.UserServices.ClientService;
using Microsoft.AspNetCore.Mvc;

namespace ITI_Project.Api.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(config =>
            {
                config.AddConsole(); // Enables console logging
                config.AddDebug();   // Enables debug output
            });
             
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            var cloudinarySettings = configuration
                .GetSection("Cloudinary")
                .Get<CloudinarySettings>();

            var useCloudinary = cloudinarySettings is not null &&
                !string.IsNullOrWhiteSpace(cloudinarySettings.CloudName) &&
                !string.IsNullOrWhiteSpace(cloudinarySettings.ApiKey) &&
                !string.IsNullOrWhiteSpace(cloudinarySettings.ApiSecret);

            if (useCloudinary)
                services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
            else
                services.AddScoped<IFileStorageService, LocalFileStorageService>();

            services.AddScoped<IPaymentService, PaymentService>();

            // Register the client service
            services.AddScoped<IClientService, ClientService>();

            services.AddScoped(typeof(ExistingIdFilter<>));
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfiles>());

            // add the ImageQualityService to the DI container
            services.AddScoped<IImageQualityService, ImageQualityService>();
            // Add the DocumentAiWorker as a hosted service
            //services.AddHostedService<DocumentAiWorker>();

            // Add the LocationService to the DI container
            services.AddHttpClient<ILocationService, LocationService>();
            // add the ImageMigrationQueue as a singleton service
            services.AddSingleton<IImageMigrationQueue, ImageMigrationQueue>();
            // add the ImageMigrationBackgroundService as a hosted service
            services.AddHostedService<ImageMigrationBackgroundService>();

            // Bind settings
            services.Configure<StripeSettings>(
                configuration.GetSection("Stripe")
            );

            services.Configure<ITI_Project.Api.Settings.CloudinarySettings>(
                configuration.GetSection("Cloudinary")
            );
            // Set Stripe API key
            var stripeSettings = configuration
                .GetSection("Stripe")
                .Get<StripeSettings>();


            Stripe.StripeConfiguration.ApiKey = stripeSettings!.SecretKey;

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = ActionContext =>
                {
                    var errors = ActionContext.ModelState
                        .Where(p => p.Value?.Errors.Count() > 0)
                        .SelectMany(p => p.Value?.Errors!)
                        .Select(e => e.ErrorMessage).ToArray();
                    var ValidationErrorResponse = new ApiValidationErrorResponse()
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(ValidationErrorResponse);
                };
            });

            return services;
        }
    }
}
