using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;

namespace Staybnb.ViewComponents
{
    // style: "Default" = bell icon link, "Badge" = just the number
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public NotificationBellViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync(string style = "Default")
        {
            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            int unread = 0;
            if (userId != null)
                unread = await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

            return View(style, unread);
        }
    }
}
