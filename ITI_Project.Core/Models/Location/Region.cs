using ITI_Project.Core.Models.Persons;
using ITI_Project.Core.Models.Posts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Location
{
    public class Region: BaseEntity
    {
        [Required(ErrorMessage = "Region name is required. Please provide a name.")]
        public string Name_ar { get; set; }

        [Required(ErrorMessage = "Region name is required. Please provide a name.")]
        public string Name_en { get; set; }

        [ForeignKey(nameof(Governorate))]
        public required int GovernorateId { get; set; }
        public required Governorate Governorate { get; set; }
        public ICollection<Provider>? Providers { get; set; }
        public ICollection<Post>? Posts { get; set; }
    }
}
