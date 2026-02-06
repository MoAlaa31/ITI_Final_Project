using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Location
{
    public class Region
    {
        [Required(ErrorMessage = "Region name is required. Please provide a name.")]
        public string Name_ar { get; set; }

        [Required(ErrorMessage = "Region name is required. Please provide a name.")]
        public string Name_en { get; set; }

        public int governorateId { get; set; }
        [ForeignKey("governorateId")]
        public Governorate governorate { get; set; }
        public ICollection<Provider> Providers { get; set; }
    }
}
