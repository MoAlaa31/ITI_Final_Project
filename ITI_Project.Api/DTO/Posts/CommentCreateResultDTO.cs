using System;

namespace ITI_Project.Api.DTO.Posts
{
    public class CommentCreateResultDTO
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int ClientId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}