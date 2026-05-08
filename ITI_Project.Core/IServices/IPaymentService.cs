using ITI_Project.Core.ServiceDTOs;

namespace ITI_Project.Core.IServices
{
    public interface IPaymentService
    {
        Task<StripePaymentIntentResultDto> CreatePaymentIntentAsync(string userId, int credits);

        Task<StripeWebhookProcessResultDto> ProcessStripeWebhookAsync(string payload, string stripeSignatureHeader);
    }
}
