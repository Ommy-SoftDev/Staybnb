using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;

namespace Staybnb.ViewComponents
{
    // style: "Default" = text link, "Icon" = envelope icon, "Badge" = just the number
    public class MessageBadgeViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public MessageBadgeViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync(string style = "Default")
        {
            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            int unread = 0;
            if (userId != null)
                unread = await _db.Messages.CountAsync(m => m.ReceiverId == userId && !m.IsRead);

            return View(style, unread);
        }
    }
}
