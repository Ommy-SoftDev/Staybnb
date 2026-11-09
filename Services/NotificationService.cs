using Staybnb.Data;
using Staybnb.Enums;
using Staybnb.Models;

namespace Staybnb.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task NotifyAsync(string userId, string title, string? body, NotificationType type)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Body = body,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }
}
