using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITI_Project.Core.Models.Posts
{
    public class PostImage: BaseEntity
    {
        public int Id { get; set; }
        public required string ImageUrl { get; set; }

        // Relationships
        [ForeignKey("Post")]
        public required int PostId { get; set; }
        public required Post Post { get; set; }
    }
}
