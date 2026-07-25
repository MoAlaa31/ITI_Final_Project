using ITI_Project.Core;
using ITI_Project.Core.Common;
using ITI_Project.Core.IServices;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Posts;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Services.Files
{
    public class ImageMigrationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IImageMigrationQueue queue;
        private readonly ILogger<ImageMigrationBackgroundService> logger;

        public ImageMigrationBackgroundService(
            IServiceProvider serviceProvider,
            IImageMigrationQueue queue,
            ILogger<ImageMigrationBackgroundService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.queue = queue;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await queue.WaitForMigrationAsync(stoppingToken);

                logger.LogInformation("Starting image migration...");

                using var scope = serviceProvider.CreateScope();

                var unitOfWork =
                    scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var environment =
                    scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                var fileStorageService =
                    scope.ServiceProvider.GetRequiredService<IFileStorageService>();

                var documents = await unitOfWork
                    .Repository<ServiceRequestImage>()
                    .GetAllAsync();

                var migratedCount = await MigrateImagesAsync(
                    documents,
                    sr => sr.ImageUrl,
                    (sr, url) => sr.ImageUrl = url,
                    "service-request-images",
                    environment,
                    fileStorageService,
                    unitOfWork,
                    logger,
                    sr => sr.Id);

                await unitOfWork.CompleteAsync();

                logger.LogInformation(
                    "Migration completed. Total migrated: {Count}",
                    migratedCount);
            }
        }

        private async Task<int> MigrateImagesAsync<TEntity>(
            IEnumerable<TEntity> entities,
            Func<TEntity, string?> getImageUrl,
            Action<TEntity, string> setImageUrl,
            string folderName,
            IWebHostEnvironment environment,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork,
            ILogger logger,
            Func<TEntity, object?> getEntityId)
        {
            var migratedCount = 0;
            var batchCount = 0;

            foreach (var entity in entities)
            {
                try
                {
                    var imageUrl = getImageUrl(entity);

                    if (string.IsNullOrWhiteSpace(imageUrl))
                        continue;

                    // Skip already migrated files
                    if (imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var localPath = Path.Combine(
                        environment.WebRootPath,
                        imageUrl.TrimStart('/')
                    );

                    if (!File.Exists(localPath))
                        continue;

                    await using var stream = File.OpenRead(localPath);

                    var uploadResult = await fileStorageService.UploadFileAsync(
                        new FileUploadRequest
                        {
                            File = stream,
                            OriginalFileName = Path.GetFileName(localPath),
                            FolderName = folderName,
                            GivenName = null,
                            NameId = null
                        });

                    if (!uploadResult.Success ||
                        string.IsNullOrWhiteSpace(uploadResult.FilePath))
                    {
                        continue;
                    }

                    setImageUrl(entity, uploadResult.FilePath);

                    migratedCount++;
                    batchCount++;

                    if (batchCount >= 20)
                    {
                        await unitOfWork.CompleteAsync();

                        batchCount = 0;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Failed migrating entity {Id}",
                        getEntityId(entity));
                }
            }

            return migratedCount;
        }
    }
}