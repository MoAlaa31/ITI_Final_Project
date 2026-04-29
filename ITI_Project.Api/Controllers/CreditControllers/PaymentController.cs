using ITI_Project.Api.DTO.Users;
using ITI_Project.Api.Helpers;
using ITI_Project.Core;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Credit;
using ITI_Project.Core.Models.Identity;
using ITI_Project.Core.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog.Core;
using Stripe;
using System.Security.Claims;
using StripeEvent = Stripe.Event;

namespace ITI_Project.Api.Controllers.CreditControllers
{
    public class PaymentController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        //private readonly ILogger<PaymentController> logger;
        private readonly StripeSettings stripeSettings;

        public PaymentController(IUnitOfWork unitOfWork, IOptions<StripeSettings> options, ILogger<PaymentController> logger)
        {
            this.unitOfWork = unitOfWork;
            //this.logger = logger;
            stripeSettings = options.Value;
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpPost("buy-credits")]
        public async Task<IActionResult> BuyCredits([FromBody] AddCreditsDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var priceTable = new Dictionary<int, decimal>
            {
                { 10, 10m },
                { 25, 23m },
                { 50, 40m }
            };

            if (!priceTable.TryGetValue(request.Credits, out var price))
                return BadRequest("Invalid credits package");

            long amountCents = (long)(price * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountCents,
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId! },
                    { "credits", request.Credits.ToString() }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return Ok(new { clientSecret = paymentIntent.ClientSecret });
        }


        [HttpPost("stripe-webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            //logger.LogInformation("Webhook received!");
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            //logger.LogInformation(json);
            var webhookSecret = stripeSettings.WebhookSecret;

            StripeEvent stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );
            }
            catch
            {
                return BadRequest();
            }

            if (stripeEvent.Type == "payment_intent.succeeded")
            {

                var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;

                var metadata = paymentIntent.Metadata;

                if (!metadata.TryGetValue("userId", out var userId) ||
                    !metadata.TryGetValue("credits", out var creditsStr) ||
                    !int.TryParse(creditsStr, out var credits))
                {
                    return BadRequest();
                }

                // Idempotency check
                var exists = await unitOfWork.Repository<Payment>()
                    .AnyAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

                if (exists)
                    return Ok();

                var client = await unitOfWork.Repository<Client>().GetByAppUserIdWithIncludesAsync(userId, c => c.Provider!);
                if (client == null)
                    NotFound();

                var provider = client?.Provider;
                if (provider == null)
                    return NotFound();

                await unitOfWork.BeginTransactionAsync();

                try
                {
                    // Add credits
                    provider.Credits += credits;

                    // Save payment
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

                    await unitOfWork.CompleteAsync();
                    await unitOfWork.CommitAsync();
                }
                catch
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }

            return Ok();
        }


    }
}
