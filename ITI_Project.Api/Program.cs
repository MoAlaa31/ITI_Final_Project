using DotNetEnv;
using ITI_Project.Api.Extensions;
using ITI_Project.Api.Middlewares;
using ITI_Project.Core.Models.Identity;
using ITI_Project.Repository.Data;
using ITI_Project.Repository.Identity;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Diagnostics.Metrics;

namespace ITI_Project.Api
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            #region Logging
            Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "Logs", "app.log"), rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            #endregion

            var builder = WebApplication.CreateBuilder(args);

            if (builder.Environment.IsDevelopment())
            {
                var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
                if (File.Exists(envPath))
                {
                    DotNetEnv.Env.Load(envPath);
                }
            }

            builder.Configuration.AddEnvironmentVariables();
            builder.Host.UseSerilog();

            #region Configure Service
            // Add services to the container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials()
                           .SetIsOriginAllowed(origin => true);
                });
            });

            builder.Services.AddControllers();
            //In Program.cs, after AddControllers
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
                options.HttpOnly = HttpOnlyPolicy.Always;
            });

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSignalR();                  // need to add this to use SignalR in the project

            #region Connection String (local | global)
            /****************************** Connection String ********************************/
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddDbContext<AppIdentityDBContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
            });

            /****************************** Global Connection String ********************************/
            //builder.Services.AddDbContext<AppDbContext>(options =>
            //{
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("DeploymentDbGlobal"));
            //});

            //builder.Services.AddDbContext<AppIdentityDBContext>(options =>
            //{
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("DeploymentIdentityDbGlobal"));
            //});
            #endregion
            /****************************** Add Application Services ********************************/
            builder.Services.AddApplicationServices();

            builder.Services.AddIdentityServices(builder.Configuration);

            //builder.Services.AddIdentity<AppUser, IdentityRole>()
            //    .AddEntityFrameworkStores<AppIdentityDBContext>()
            //    .AddDefaultTokenProviders();

            #endregion
            var app = builder.Build();

            #region Update-Database auto 
            await using var scope = app.Services.CreateAsyncScope();
            var services = scope.ServiceProvider;

            // Create objects of the required services
            var applicationDbContext = services.GetRequiredService<AppDbContext>();
            var _identityContext = services.GetRequiredService<AppIdentityDBContext>();
            //var _userManager = services.GetRequiredService<UserManager<AppUser>>();
            var _roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            var factoryLogger = services.GetRequiredService<ILoggerFactory>();
            try
            {
                await applicationDbContext.Database.MigrateAsync();
                await _identityContext.Database.MigrateAsync();

                //await AppDbContextSeed.SeedAsync(applicationDbContext, factoryLogger.CreateLogger(nameof(AppDbContextSeed))); //seed the data of the application (Categories, Products, etc.)
                await RoleSeed.RoleSeedAsync(_roleManager);         //seed the roles (Admin, Provider)
            }
            catch (Exception ex)
            {
                var logger = factoryLogger.CreateLogger<Program>();
                logger.LogError(ex, "An error occurred during migration");
            }

            #endregion
            #region Configure kestrel Middlewares
            //Middlewares of Exception Handling 
            app.UseMiddleware<ExceptionMiddleware>();

            // Not found Endpoint
            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCors("CorsPolicy");
            // Then in the middleware pipeline, after UseCors and before UseAuthentication
            app.UseCookiePolicy();
            //app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<Hubs.LiveLocationHub>("/hubs/live-location");

            #endregion
            app.Run();
        }
    }
}