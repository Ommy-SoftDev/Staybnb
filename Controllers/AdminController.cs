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
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ActivityLogger _logger;
        private readonly NotificationService _notifier;

        public AdminController(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            ActivityLogger logger,
            NotificationService notifier)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _notifier = notifier;
        }

        // ---------- Dashboard ----------
        public async Task<IActionResult> Dashboard()
        {
            var hosts = await _userManager.GetUsersInRoleAsync("Host");

            var vm = new AdminDashboardVM
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                PendingApplications = await _db.HostApplications
                    .CountAsync(a => a.Status == ApplicationStatus.Pending),
                ActiveProperties = await _db.HostProperties.CountAsync(p => p.IsActive),
                TotalBookings = await _db.Bookings.CountAsync(),
                PendingBookings = await _db.Bookings
                    .CountAsync(b => b.Status == BookingStatus.Pending),
                ApprovedHosts = hosts.Count,
                PlatformRevenue = await _db.Payments
                    .Where(p => p.Status == PaymentStatus.Paid)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0,
                RecentApplications = await _db.HostApplications
                    .Include(a => a.ApplicationUser)
                    .Include(a => a.Property)
                    .OrderByDescending(a => a.AppliedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentActivity = await _db.ActivityLogs
                    .Include(l => l.User)
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(8)
                    .ToListAsync(),
                RecentBookings = await _db.Bookings
                    .Include(b => b.Guest)
                    .Include(b => b.Property)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }

        // ---------- Host applications ----------
        public async Task<IActionResult> Applications(string? status)
        {
            var query = _db.HostApplications
                .Include(a => a.ApplicationUser)
                .Include(a => a.Property)
                .AsQueryable();

            if (Enum.TryParse<ApplicationStatus>(status, out var parsed))
                query = query.Where(a => a.Status == parsed);

            ViewBag.Status = status;

            var list = await query
                .OrderBy(a => a.Status)
                .ThenByDescending(a => a.AppliedAt)
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> ApplicationDetails(int id)
        {
            var app = await _db.HostApplications
                .Include(a => a.ApplicationUser)
                .Include(a => a.Property).ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null) return NotFound();
            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var app = await _db.HostApplications
                .Include(a => a.ApplicationUser)
                .Include(a => a.Property)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null) return NotFound();

            if (app.Status != ApplicationStatus.Pending)
            {
                TempData["Error"] = "This application has already been reviewed.";
                return RedirectToAction(nameof(Applications));
            }

            app.Status = ApplicationStatus.Approved;
            app.Property.IsActive = true;
            await _db.SaveChangesAsync();

            // The role swap: add Host, remove Guest
            var user = app.ApplicationUser;
            if (!await _userManager.IsInRoleAsync(user, "Host"))
                await _userManager.AddToRoleAsync(user, "Host");
            if (await _userManager.IsInRoleAsync(user, "Guest"))
                await _userManager.RemoveFromRoleAsync(user, "Guest");

            // Forces the user to log in again so the new role takes effect
            await _userManager.UpdateSecurityStampAsync(user);

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _logger.LogAsync(adminId,
                $"Approved application #{id} for {user.Email}: role changed Guest to Host",
                ActivityType.RoleChange);

            await _notifier.NotifyAsync(user.Id, "Host application approved",
                $"Your application for '{app.Property.Title}' was approved. Log in again to open your Host dashboard.",
                NotificationType.HostApplication);

            TempData["Success"] = $"Approved. {user.Email} is now a Host.";
            return RedirectToAction(nameof(Applications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var app = await _db.HostApplications
                .Include(a => a.ApplicationUser)
                .Include(a => a.Property)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null) return NotFound();

            if (app.Status != ApplicationStatus.Pending)
            {
                TempData["Error"] = "This application has already been reviewed.";
                return RedirectToAction(nameof(Applications));
            }

            app.Status = ApplicationStatus.Rejected;
            app.Property.IsActive = false;
            await _db.SaveChangesAsync();

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _logger.LogAsync(adminId,
                $"Rejected application #{id} for {app.ApplicationUser.Email}",
                ActivityType.Application);

            await _notifier.NotifyAsync(app.ApplicationUser.Id, "Host application rejected",
                $"Your application for '{app.Property.Title}' was not approved.",
                NotificationType.HostApplication);

            TempData["Success"] = "Application rejected.";
            return RedirectToAction(nameof(Applications));
        }

        // ---------- System audit ----------
        public async Task<IActionResult> ActivityLogs()
        {
            var logs = await _db.ActivityLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .Take(200)
                .ToListAsync();

            return View(logs);
        }

        // ---------- All bookings ----------
        public async Task<IActionResult> Bookings(string? status)
        {
            var query = _db.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Property)
                .Include(b => b.GuestCheckIn)
                .AsQueryable();

            if (Enum.TryParse<BookingStatus>(status, out var parsed))
                query = query.Where(b => b.Status == parsed);

            ViewBag.Status = status;

            var list = await query
                .OrderByDescending(b => b.CreatedAt)
                .Take(200)
                .ToListAsync();

            return View(list);
        }

        // ---------- All properties ----------
        public async Task<IActionResult> Properties()
        {
            var properties = await _db.HostProperties
                .Include(p => p.Host)
                .Include(p => p.Images)
                .Include(p => p.Bookings)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var approvedIds = await _db.HostApplications
                .Where(a => a.Status == ApplicationStatus.Approved)
                .Select(a => a.PropertyId)
                .ToListAsync();

            ViewBag.ApprovedIds = approvedIds.ToHashSet();
            return View(properties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProperty(int id)
        {
            var property = await _db.HostProperties.FirstOrDefaultAsync(p => p.Id == id);
            if (property == null) return NotFound();

            bool approved = await _db.HostApplications.AnyAsync(a =>
                a.PropertyId == id && a.Status == ApplicationStatus.Approved);

            if (!approved)
            {
                TempData["Error"] = "Only approved properties can be switched on or off.";
                return RedirectToAction(nameof(Properties));
            }

            property.IsActive = !property.IsActive;
            await _db.SaveChangesAsync();

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _logger.LogAsync(adminId,
                $"{(property.IsActive ? "Activated" : "Deactivated")} property '{property.Title}' as admin",
                ActivityType.Other);

            await _notifier.NotifyAsync(property.HostId,
                property.IsActive ? "Property activated" : "Property deactivated",
                $"An admin {(property.IsActive ? "activated" : "deactivated")} '{property.Title}'.",
                NotificationType.General);

            TempData["Success"] = $"'{property.Title}' is now {(property.IsActive ? "active" : "inactive")}.";
            return RedirectToAction(nameof(Properties));
        }

        // ---------- User management (SuperAdmin only) ----------
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Users(string? q)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(u =>
                    u.Email!.Contains(q) || u.FirstName.Contains(q) || u.LastName.Contains(q));
            }

            var users = await query.OrderBy(u => u.Email).Take(100).ToListAsync();

            var rows = new List<UserRowVM>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                rows.Add(new UserRowVM
                {
                    Id = u.Id,
                    Name = $"{u.FirstName} {u.LastName}".Trim(),
                    Email = u.Email ?? "",
                    Roles = roles.ToList()
                });
            }

            ViewBag.Query = q;
            return View(rows);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!await _userManager.IsInRoleAsync(user, "Guest"))
            {
                TempData["Error"] = "Only guests can be promoted to Admin.";
                return RedirectToAction(nameof(Users));
            }

            await _userManager.AddToRoleAsync(user, "Admin");
            await _userManager.RemoveFromRoleAsync(user, "Guest");
            await _userManager.UpdateSecurityStampAsync(user);

            var superAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _logger.LogAsync(superAdminId,
                $"Promoted {user.Email} from Guest to Admin", ActivityType.RoleChange);

            await _notifier.NotifyAsync(user.Id, "You are now an Admin",
                "A SuperAdmin promoted your account. Log in again to open the Admin dashboard.",
                NotificationType.General);

            TempData["Success"] = $"{user.Email} is now an Admin.";
            return RedirectToAction(nameof(Users));
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["Error"] = "Type a new password first.";
                return RedirectToAction(nameof(Users));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Users));
            }

            var superAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _logger.LogAsync(superAdminId,
                $"Reset the password for {user.Email}", ActivityType.Other);

            TempData["Success"] = $"Password reset for {user.Email}.";
            return RedirectToAction(nameof(Users));
        }
    }
}
