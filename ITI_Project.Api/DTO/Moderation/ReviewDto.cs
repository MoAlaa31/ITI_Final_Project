using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.DTO.Moderation
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public double Rating { get; set; }
        public string? Message { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientPictureUrl { get; set; }
    }

    public class ReviewCreateDto
    {
        [Required]
        public int ServiceRequestId { get; set; }

        [Range(0, 5)]
        [Required]
        public double Rating { get; set; }

        [StringLength(200)]
        public string? Message { get; set; }
    }

    public class ReviewUpdateDto
    {
        [Range(0, 5)]
        [Required]
        public double Rating { get; set; }

        [StringLength(200)]
        public string? Message { get; set; }
    }
}