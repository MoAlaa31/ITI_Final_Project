using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Requests
{
    public class RequestOfferFromUserDTO
    {
        [Range(0.0, 1000000.0, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }

        [StringLength(200, ErrorMessage = "Message cannot be longer than 200 characters.")]
        [Required(ErrorMessage = "Message is required.")]
        public required string Message { get; set; }
    }
}
