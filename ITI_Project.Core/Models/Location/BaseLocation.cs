using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Location
{
    public class BaseLocation: BaseEntity
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? AddressText { get; set; }


        [ForeignKey(nameof(Provider))]
        public int ProviderId { get; set; }
        public Provider Provider { get; set; }
    }
}
