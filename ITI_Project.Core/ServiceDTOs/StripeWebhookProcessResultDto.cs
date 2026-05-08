namespace ITI_Project.Core.ServiceDTOs
{
    public class StripeWebhookProcessResultDto
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
