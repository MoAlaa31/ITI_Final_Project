using ITI_Project.Core.Models.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ITI_Project.Core.Models.Location
{
    public class Governorate
    {
        [Required(ErrorMessage = "Governorate name is required. Please provide a name.")]
        public string Name_ar { get; set; }
        [Required(ErrorMessage = "Governorate name is required. Please provide a name.")]
        public string Name_en { get; set; }

        // Relations
        public ICollection<Region> regions { get; set; }
        public ICollection<Provider> Providers { get; set; }
    }
}
