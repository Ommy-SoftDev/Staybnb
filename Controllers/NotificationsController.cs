using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Enums;
using Staybnb.Models;
using Staybnb.ViewModels;

namespace Staybnb.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public NotificationsController(ApplicationDbContext db)
        {
            _db = db;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // The list on the left, the open notification on the right.
        // Opening one marks it as read in the database.
        public async Task<IActionResult> Index(int? selectedId, string? filter)
        {
            var me = CurrentUserId;

            var all = await _db.Notifications
                .Where(n => n.UserId == me)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            Notification? selected = null;
            if (selectedId.HasValue)
            {
                selected = all.FirstOrDefault(n => n.Id == selectedId.Value);
                if (selected != null && !selected.IsRead)
                {
                    selected.IsRead = true;
                    await _db.SaveChangesAsync();
                }
            }

            var items = all;
            if (filter == "unread")
            {
                items = all.Where(n => !n.IsRead || (selected != null && n.Id == selected.Id)).ToList();
            }

            return View(new NotificationsVM
            {
                Items = items,
                Selected = selected,
                ActionUrl = selected == null ? null : ActionUrlFor(selected),
                Filter = filter == "unread" ? "unread" : "all",
                UnreadCount = all.Count(n => !n.IsRead),
                TotalCount = all.Count
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var unread = await _db.Notifications
                .Where(n => n.UserId == CurrentUserId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread) n.IsRead = true;
            await _db.SaveChangesAsync();

            TempData["Success"] = "All notifications marked as read.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == CurrentUserId);
            if (n == null) return NotFound();

            _db.Notifications.Remove(n);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Where the "Take action" button goes, depending on the type and the user's role
        private string? ActionUrlFor(Notification n)
        {
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            bool isHost = User.IsInRole("Host");

            switch (n.Type)
            {
                case NotificationType.NewMessage:
                    return Url.Action("Inbox", "Messages");

                case NotificationType.BookingApproved:
                case NotificationType.BookingRejected:
                    return Url.Action("MyBookings", "Bookings");

                case NotificationType.HostApplication:
                    if (isAdmin) return Url.Action("Applications", "Admin");
                    if (isHost) return Url.Action("MyProperties", "Host");
                    return Url.Action("Status", "HostApplication");

                default:
                    if (isAdmin) return Url.Action("Dashboard", "Admin");
                    if (isHost) return Url.Action("Index", "HostBookings");
                    return Url.Action("MyBookings", "Bookings");
            }
        }
    }
}
