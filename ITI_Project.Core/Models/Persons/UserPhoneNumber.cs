using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Persons
{
    public class UserPhoneNumber: BaseEntity
    {
        public int Id { get; set; }
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public required string PhoneNumber { get; set; }

        // Relationships
        [ForeignKey("User")]
        public required int UserId { get; set; }
        public required User User { get; set; }
    }
}
