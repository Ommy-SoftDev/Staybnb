using System.ComponentModel.DataAnnotations;
using Staybnb.Enums;

namespace Staybnb.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public string? Body { get; set; }
        public NotificationType Type { get; set; } = NotificationType.General;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}