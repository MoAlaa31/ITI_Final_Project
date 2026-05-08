using AutoMapper;
using ITI_Project.Api.DTO.Credit;
using ITI_Project.Api.DTO.Users;
using ITI_Project.Api.Helpers;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.IServices;
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
        private readonly IMapper mapper;
        private readonly IPaymentService paymentService;

        public PaymentController(IUnitOfWork unitOfWork, ILogger<PaymentController> logger, IMapper mapper, IPaymentService paymentService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.paymentService = paymentService;
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpPost("buy-credits")]
        public async Task<IActionResult> BuyCredits([FromBody] AddCreditsDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await paymentService.CreatePaymentIntentAsync(userId, request.Credits);
            return Ok(new { clientSecret = result.ClientSecret });
        }

        [HttpPost("stripe-webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            var stripeSignatureHeader = Request.Headers["Stripe-Signature"].ToString();
            var result = await paymentService.ProcessStripeWebhookAsync(json, stripeSignatureHeader);

            if (!result.Success)
            {
                if (string.IsNullOrWhiteSpace(result.ErrorMessage))
                    return StatusCode(result.StatusCode);

                return StatusCode(result.StatusCode, result.ErrorMessage);
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
