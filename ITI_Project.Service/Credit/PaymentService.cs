using ITI_Project.Api.Settings;
using ITI_Project.Core;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Helpers;
using ITI_Project.Core.IServices;
using ITI_Project.Core.Models.Credit;
using ITI_Project.Core.Models.Users;
using ITI_Project.Core.ServiceDTOs;
using Microsoft.Extensions.Options;
using Stripe;
using StripeEvent = Stripe.Event;

namespace ITI_Project.Services.Credit
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly StripeSettings stripeSettings;

        public PaymentService(IUnitOfWork unitOfWork, IOptions<StripeSettings> options)
        {
            this.unitOfWork = unitOfWork;
            stripeSettings = options.Value;
        }

        public async Task<StripePaymentIntentResultDto> CreatePaymentIntentAsync(string userId, int credits)
        {
            var priceTable = new Dictionary<int, decimal>
            {
                { 50, 50m },
                { 100, 100m },
                { 200, 200m }
            };

            if (!priceTable.TryGetValue(credits, out var price))
                throw new ArgumentOutOfRangeException(nameof(credits), "Invalid credits package");

            long amountCents = (long)(price * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountCents,
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId },
                    { "credits", credits.ToString() }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return new StripePaymentIntentResultDto
            {
                ClientSecret = paymentIntent.ClientSecret
            };
        }

        public async Task<StripeWebhookProcessResultDto> ProcessStripeWebhookAsync(string payload, string stripeSignatureHeader)
        {
            StripeEvent stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    payload,
                    stripeSignatureHeader,
                    stripeSettings.WebhookSecret);
            }
            catch
            {
                return new StripeWebhookProcessResultDto
                {
                    Success = false,
                    StatusCode = 400,
                    ErrorMessage = "Invalid Stripe signature"
                };
            }

            if (stripeEvent.Type != "payment_intent.succeeded")
            {
                return new StripeWebhookProcessResultDto
                {
                    Success = true,
                    StatusCode = 200
                };
            }

            var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
            var metadata = paymentIntent.Metadata;

            if (!metadata.TryGetValue("userId", out var userId) ||
                !metadata.TryGetValue("credits", out var creditsStr) ||
                !int.TryParse(creditsStr, out var credits))
            {
                return new StripeWebhookProcessResultDto
                {
                    Success = false,
                    StatusCode = 400,
                    ErrorMessage = "Missing metadata"
                };
            }

            // Idempotency check
            var exists = await unitOfWork.Repository<Payment>()
                .AnyAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

            if (exists)
            {
                return new StripeWebhookProcessResultDto
                {
                    Success = true,
                    StatusCode = 200
                };
            }

            var client = await unitOfWork.Repository<Client>()
                .GetByAppUserIdWithIncludesAsync(userId, c => c.Provider!);

            var provider = client?.Provider;
            if (provider == null)
            {
                return new StripeWebhookProcessResultDto
                {
                    Success = false,
                    StatusCode = 404,
                    ErrorMessage = "Provider not found"
                };
            }

            await unitOfWork.BeginTransactionAsync();

            try
            {
                provider.Credits += credits;

                var payment = new Payment
                {
                    StripePaymentIntentId = paymentIntent.Id,
                    UserId = userId,
                    Credits = credits,
                    Amount = paymentIntent.Amount / 100m,
                    Status = PaymentStatus.Completed,
                    CreatedAt = DateHelper.GetNowInEgypt()
                };

                await unitOfWork.Repository<Payment>().AddAsync(payment);

                await unitOfWork.Repository<CreditTransaction>().AddAsync(new CreditTransaction
                {
                    ProviderId = provider.Id,
                    Amount = credits,
                    Type = TransactionType.Purchase,
                    ReferenceId = paymentIntent.Id,
                    CreatedAt = DateHelper.GetNowInEgypt()
                });

                unitOfWork.Repository<Provider>().Update(provider);

                await unitOfWork.CompleteAsync();
                await unitOfWork.CommitAsync();

                return new StripeWebhookProcessResultDto
                {
                    Success = true,
                    StatusCode = 200
                };
            }
            catch
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
