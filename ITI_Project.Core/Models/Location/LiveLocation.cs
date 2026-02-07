using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Location
{
    public class LiveLocation: BaseEntity
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Relationships
        [ForeignKey(nameof(Provider))]
        public required int ProviderId { get; set; }
        public required Provider Provider { get; set; }
    }
}
