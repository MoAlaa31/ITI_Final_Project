using System.ComponentModel.DataAnnotations.Schema;

namespace ITI_Project.Core.Models.Requests
{
    public class ServiceRequestImage : BaseEntity
    {
        public int Id { get; set; }
        public required string ImageUrl { get; set; }

        [ForeignKey(nameof(ServiceRequest))]
        public required int ServiceRequestId { get; set; }
        public ServiceRequest ServiceRequest { get; set; } = null!;
    }
}