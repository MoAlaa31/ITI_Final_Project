using ITI_Project.Core.IServices;
using ITI_Project.Core.Models.Identity;
using ITI_Project.Repository.Identity;
using ITI_Project.Services.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ITI_Project.Api.Extensions
{
    public static class IdentityServicesExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IAuthService), typeof(AuthService));
            services.AddHttpContextAccessor();
            //services.AddScoped(typeof(IUserService), typeof(UserService));


            // add Identity Services configuration (UserManager , SigninManager , RoleManager)
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                //options.Password.RequiredUniqueChars = 2;
                //options.Password.RequireNonAlphanumeric = true;
                //options.Password.RequireUppercase = true;
                //options.Password.RequireLowercase = true;
                //options.SignIn.RequireConfirmedEmail = true;
                //options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                //options.Lockout.MaxFailedAccessAttempts = 5;
                //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

            }).AddEntityFrameworkStores<AppIdentityDBContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<DataProtectorTokenProvider<AppUser>>("Email");

            // AddAuthentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; //default scheme for other auth
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:ValidAudience"],
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"])),
                    ValidateLifetime = true,
                    //ClockSkew = TimeSpan.FromDays(double.Parse(configuration["JWT:DurationInDays"])),
                    ClockSkew = TimeSpan.Zero
                };

                //options.Events = new JwtBearerEvents
                //{
                //    OnMessageReceived = context =>
                //    {
                //        var accessToken = context.Request.Query["access_token"];

                //        // Allow JWT token via WebSockets
                //        if (!string.IsNullOrEmpty(accessToken) &&
                //            context.HttpContext.Request.Path.StartsWithSegments("/Hubs/NotificationHub"))           // SignalR for notifications
                //        {
                //            context.Token = accessToken;
                //        }

                //        return Task.CompletedTask;
                //    }
                //OnAuthenticationFailed = context =>
                //{
                //    var logger = context.HttpContext.RequestServices
                //        .GetRequiredService<ILoggerFactory>()
                //        .CreateLogger("JwtBearer");
                //    logger.LogError(context.Exception, "JWT authentication failed.");
                //    return Task.CompletedTask;
                //},
                //    OnChallenge = context =>
                //    {
                //        var logger = context.HttpContext.RequestServices
                //            .GetRequiredService<ILoggerFactory>()
                //            .CreateLogger("JwtBearer");
                //        logger.LogWarning("JWT challenge triggered. Error: {Error}, Description: {Description}", context.Error, context.ErrorDescription);
                //        return Task.CompletedTask;
                //    }
                //};
            });

            return services;
        }
    }
}
