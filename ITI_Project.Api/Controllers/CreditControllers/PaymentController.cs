using AutoMapper;
using ITI_Project.Api.DTO.Credit;
using ITI_Project.Api.DTO.Users;
using ITI_Project.Api.Helpers;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Credit;
using ITI_Project.Core.Models.Identity;
using ITI_Project.Core.Models.Users;
using ITI_Project.Core.Specifications;
using ITI_Project.Core.Specifications.CreditSpecs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using System.Security.Claims;
using StripeEvent = Stripe.Event;

namespace ITI_Project.Api.Controllers.CreditControllers
{
    public class PaymentController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly StripeSettings stripeSettings;
        private readonly IMapper mapper;

        public PaymentController(IUnitOfWork unitOfWork, IOptions<StripeSettings> options, ILogger<PaymentController> logger, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            stripeSettings = options.Value;
            this.mapper = mapper;
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpPost("buy-credits")]
        public async Task<IActionResult> BuyCredits([FromBody] AddCreditsDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var priceTable = new Dictionary<int, decimal>
            {
                { 50, 50m },
                { 100, 100m },
                { 200, 200m }
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
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
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

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpGet("payments")]
        public async Task<IActionResult> GetMyPayments([FromQuery] PaginationSpecParams specParams)
        {
            specParams.SetMaxPageSize(20);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var countSpec = new BaseSpecifications<Payment>(p => p.UserId == userId);
            var count = await unitOfWork.Repository<Payment>().GetCountAsync(countSpec);

            var spec = new PaymentWithPaginationSpecification(userId, specParams);

            var payments = await unitOfWork.Repository<Payment>()
                .GetAllWithSpecAsync(spec) ?? new List<Payment>();

            var data = mapper.Map<List<PaymentDTO>>(payments);

            return Ok(new Pagination<PaymentDTO>(specParams.PageIndex, specParams.PageSize, count, data));
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpGet("credit-transactions")]
        public async Task<IActionResult> GetMyCreditTransactions([FromQuery] PaginationSpecParams specParams)
        {
            specParams.SetMaxPageSize(20);

            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out var providerId))
                return Unauthorized();

            var countSpec = new BaseSpecifications<CreditTransaction>(t => t.ProviderId == providerId);
            var count = await unitOfWork.Repository<CreditTransaction>().GetCountAsync(countSpec);

            var spec = new CreditTransactionWithPaginationSpecification(providerId, specParams);

            var transactions = await unitOfWork.Repository<CreditTransaction>()
                .GetAllWithSpecAsync(spec) ?? new List<CreditTransaction>();

            var data = mapper.Map<List<CreditTransactionDTO>>(transactions);

            return Ok(new Pagination<CreditTransactionDTO>(specParams.PageIndex, specParams.PageSize, count, data));
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpGet("payment-by-intent/{intentId}")]
        public async Task<IActionResult> GetMyPaymentByIntentId(string intentId)
        {
            if (string.IsNullOrWhiteSpace(intentId))
                return BadRequest("intentId is required");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var payment = await unitOfWork.Repository<Payment>()
                .GetByConditionAsync(p =>
                    p.UserId == userId &&
                    p.StripePaymentIntentId == intentId);

            if (payment == null)
                return NotFound();

            var dto = mapper.Map<PaymentDTO>(payment);
            return Ok(dto);
        }
    }
}
