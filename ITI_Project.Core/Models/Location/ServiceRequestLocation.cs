using ITI_Project.Core.Models.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Location
{
    public class ServiceRequestLocation: BaseEntity
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        [StringLength(100, ErrorMessage = "Address cannot be longer than 100 characters.")]
        public string? AddressText { get; set; }

        // Relationships
        [ForeignKey(nameof(ServiceRequest))]
        public required int ServiceRequestId { get; set; }
        public required ServiceRequest ServiceRequest { get; set; }
    }
}
