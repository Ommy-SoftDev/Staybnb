using System.ComponentModel.DataAnnotations;
using Staybnb.Enums;

namespace Staybnb.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required, StringLength(200)]
        public string Action { get; set; } = string.Empty;

        public ActivityType ActivityType { get; set; } = ActivityType.Other;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}