using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Enums;
using Staybnb.Models;
using Staybnb.Services;
using Staybnb.ViewModels;

namespace Staybnb.Controllers
{
    // A database-backed inbox. Not real time: people open the page to read and reply.
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notifier;

        public MessagesController(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            NotificationService notifier)
        {
            _db = db;
            _userManager = userManager;
            _notifier = notifier;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ---------- Inbox: one row per conversation ----------
        public async Task<IActionResult> Inbox()
        {
            var me = CurrentUserId;

            var messages = await _db.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == me || m.ReceiverId == me)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            var rows = messages
                .GroupBy(m => m.SenderId == me ? m.ReceiverId : m.SenderId)
                .Select(g =>
                {
                    var last = g.First();
                    var other = last.SenderId == me ? last.Receiver : last.Sender;
                    return new ConversationRowVM
                    {
                        OtherUserId = g.Key,
                        OtherName = NameHelper.Display(other),
                        LastMessage = last.Content.Length > 80 ? last.Content.Substring(0, 80) + "..." : last.Content,
                        LastTime = last.Timestamp,
                        UnreadCount = g.Count(m => m.ReceiverId == me && !m.IsRead)
                    };
                })
                .OrderByDescending(r => r.LastTime)
                .ToList();

            return View(rows);
        }

        // ---------- One conversation. Opening it marks incoming messages as read ----------
        public async Task<IActionResult> Conversation(string userId)
        {
            var me = CurrentUserId;

            if (userId == me) return RedirectToAction(nameof(Inbox));

            var other = await _userManager.FindByIdAsync(userId);
            if (other == null) return NotFound();

            var thread = await _db.Messages
                .Where(m => (m.SenderId == me && m.ReceiverId == userId) ||
                            (m.SenderId == userId && m.ReceiverId == me))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            var unread = thread.Where(m => m.ReceiverId == me && !m.IsRead).ToList();
            if (unread.Any())
            {
                foreach (var m in unread) m.IsRead = true;
                await _db.SaveChangesAsync();
            }

            return View(new ConversationVM
            {
                CurrentUserId = me,
                OtherUserId = userId,
                OtherName = NameHelper.Display(other),
                Messages = thread
            });
        }

        // ---------- Send a message or a reply ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string receiverId, string content)
        {
            var me = CurrentUserId;
            content = (content ?? "").Trim();

            if (receiverId == me) return RedirectToAction(nameof(Inbox));

            var receiver = await _userManager.FindByIdAsync(receiverId);
            if (receiver == null) return NotFound();

            if (content.Length == 0)
            {
                TempData["Error"] = "Type a message first.";
                return RedirectToAction(nameof(Conversation), new { userId = receiverId });
            }

            if (content.Length > 2000)
            {
                TempData["Error"] = "Messages can be up to 2000 characters.";
                return RedirectToAction(nameof(Conversation), new { userId = receiverId });
            }

            _db.Messages.Add(new Message
            {
                SenderId = me,
                ReceiverId = receiverId,
                Content = content,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            });
            await _db.SaveChangesAsync();

            var sender = await _userManager.FindByIdAsync(me);
            var title = "New message from " + NameHelper.Display(sender);
            if (title.Length > 100) title = title.Substring(0, 100);

            await _notifier.NotifyAsync(receiverId, title,
                content.Length > 100 ? content.Substring(0, 100) + "..." : content,
                NotificationType.NewMessage);

            return RedirectToAction(nameof(Conversation), new { userId = receiverId });
        }
    }
}
