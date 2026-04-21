using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.Models.Moderation
{
    public class Notification
    {
        public int Id { get; set; }
        public required string Title { get; set; } = null!;
        public required string Message { get; set; } = null!;
        public required NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;

        [ForeignKey(nameof(Client))]
        public required int ClientId { get; set; }
        public Client Client { get; set; } = null!;
    }
}
