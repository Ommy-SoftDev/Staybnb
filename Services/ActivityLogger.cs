using Staybnb.Data;
using Staybnb.Enums;
using Staybnb.Models;

namespace Staybnb.Services
{
    public class ActivityLogger
    {
        private readonly ApplicationDbContext _db;

        public ActivityLogger(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string? userId, string action, ActivityType type)
        {
            _db.ActivityLogs.Add(new ActivityLog
            {
                UserId = userId,
                Action = action,
                ActivityType = type,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }
}
