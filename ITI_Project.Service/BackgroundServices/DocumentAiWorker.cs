using ITI_Project.Core;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Users;
using ITI_Project.Services.AzureAi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace ITI_Project.Services.BackgroundServices
{
    public class DocumentAiWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DocumentAiWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var unitOfWork =
                    scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var imageService =
                    scope.ServiceProvider.GetRequiredService<IImageQualityService>();

                var documents = await unitOfWork.Repository<ProviderDocument>()
                    .GetManyByConditionAsync(d =>
                        d.IsApproved == null &&
                        !d.IsAiProcessing &&
                        !d.IsAiReviewed
                    );

                foreach (var doc in documents ?? Enumerable.Empty<ProviderDocument>())
                {
                    try
                    {
                        doc.IsAiProcessing = true;

                        unitOfWork.Repository<ProviderDocument>()
                            .Update(doc);

                        await unitOfWork.CompleteAsync();

                        var isClear =
                            imageService.IsImageClear(doc.DocumentUrl);

                        if (!isClear)
                        {
                            doc.IsApproved = false;

                            unitOfWork.Repository<ProviderDocument>()
                                .Update(doc);

                            var provider =
                                await unitOfWork.Repository<Provider>()
                                    .GetByIdAsync(doc.ProviderId);

                            if (provider is not null)
                            {
                                provider.VerificationStatus =
                                    VerificationStatus.Pending;

                                unitOfWork.Repository<Provider>()
                                    .Update(provider);
                            }
                        }
                    }
                    catch
                    {

                    }
                    finally
                    {
                        doc.IsAiProcessing = false;
                        doc.IsAiReviewed = true;

                        unitOfWork.Repository<ProviderDocument>()
                            .Update(doc);

                        await unitOfWork.CompleteAsync();
                    }
                }

                await Task.Delay(10_000, stoppingToken);
            }
        }
    }
}
