using ITI_Project.Api.Extensions;
using ITI_Project.Repository.Data;
using ITI_Project.Repository.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ITI_Project.Api
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            #region Configure Service
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
            #endregion
            /****************************** Add Application Services ********************************/
            builder.Services.AddApplicationServices();
            #endregion
            var app = builder.Build();

            #region Update-Database auto 
            await using var scope = app.Services.CreateAsyncScope();
            var services = scope.ServiceProvider;

            var applicationDbContext = services.GetRequiredService<AppDbContext>();
            var _identityContext = services.GetRequiredService<AppIdentityDBContext>();

            var factoryLogger = services.GetRequiredService<ILoggerFactory>();
            try
            {
                await applicationDbContext.Database.MigrateAsync();
                await _identityContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                var logger = factoryLogger.CreateLogger<Program>();
                logger.LogError(ex, "An error occurred during migration");
            }
            #endregion
            // Configure the HTTP request pipeline.

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}