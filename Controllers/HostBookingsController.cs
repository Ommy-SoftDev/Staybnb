using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Enums;
using Staybnb.Models;
using Staybnb.Services;
using Staybnb.ViewModels;

namespace Staybnb.Controllers
{
    [Authorize(Roles = "Host")]
    public class HostBookingsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ActivityLogger _logger;
        private readonly NotificationService _notifier;

        public HostBookingsController(ApplicationDbContext db, ActivityLogger logger, NotificationService notifier)
        {
            _db = db;
            _logger = logger;
            _notifier = notifier;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ---------- Booking requests ----------
        public async Task<IActionResult> Index(string? status)
        {
            var query = _db.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Property)
                .Include(b => b.GuestCheckIn)
                .Where(b => b.Property.HostId == CurrentUserId);

            if (Enum.TryParse<BookingStatus>(status, out var parsed))
                query = query.Where(b => b.Status == parsed);

            ViewBag.Status = status;

            var list = await query
                .OrderBy(b => b.Status)
                .ThenByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var booking = await _db.Bookings
                .Include(b => b.Property)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == id && b.Property.HostId == CurrentUserId);
            if (booking == null) return NotFound();

            if (booking.Status != BookingStatus.Pending)
            {
                TempData["Error"] = "This booking has already been dealt with.";
                return RedirectToAction(nameof(Index));
            }

            booking.Status = BookingStatus.Approved;
            if (booking.Payment != null) booking.Payment.Status = PaymentStatus.Paid;
            await _db.SaveChangesAsync();

            await _logger.LogAsync(CurrentUserId,
                $"Approved booking #{booking.Id} for '{booking.Property.Title}'",
                ActivityType.BookingStatus);

            await _notifier.NotifyAsync(booking.GuestId, "Booking approved",
                $"Your booking for '{booking.Property.Title}' was approved. Complete your check-in from My Bookings.",
                NotificationType.BookingApproved);

            TempData["Success"] = "Booking approved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var booking = await _db.Bookings
                .Include(b => b.Property)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == id && b.Property.HostId == CurrentUserId);
            if (booking == null) return NotFound();

            if (booking.Status != BookingStatus.Pending)
            {
                TempData["Error"] = "This booking has already been dealt with.";
                return RedirectToAction(nameof(Index));
            }

            booking.Status = BookingStatus.Rejected;
            if (booking.Payment != null) booking.Payment.Status = PaymentStatus.Failed;
            await _db.SaveChangesAsync();

            await _logger.LogAsync(CurrentUserId,
                $"Rejected booking #{booking.Id} for '{booking.Property.Title}'",
                ActivityType.BookingStatus);

            await _notifier.NotifyAsync(booking.GuestId, "Booking rejected",
                $"Your booking request for '{booking.Property.Title}' was not accepted.",
                NotificationType.BookingRejected);

            TempData["Success"] = "Booking rejected.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Review the guest's check-in documents ----------
        public async Task<IActionResult> Review(int id)
        {
            var booking = await _db.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Property).ThenInclude(p => p.CheckInProcess)
                .Include(b => b.GuestCheckIn).ThenInclude(c => c!.Documents)
                .AsSplitQuery()
                .FirstOrDefaultAsync(b => b.Id == id && b.Property.HostId == CurrentUserId);
            if (booking == null) return NotFound();

            return View(new HostReviewVM
            {
                Booking = booking,
                Config = CheckInHelper.Read(booking.Property.CheckInProcess)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> VerifyDocument(int id) => DecideAsync(id, true);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> RejectDocument(int id) => DecideAsync(id, false);

        private async Task<IActionResult> DecideAsync(int documentId, bool verify)
        {
            var checkIn = await _db.GuestCheckIns
                .Include(c => c.Documents)
                .Include(c => c.CheckInProcess)
                .Include(c => c.Booking).ThenInclude(b => b.Property)
                .FirstOrDefaultAsync(c =>
                    c.Documents.Any(d => d.Id == documentId) &&
                    c.Booking.Property.HostId == CurrentUserId);
            if (checkIn == null) return NotFound();

            var booking = checkIn.Booking;

            if (checkIn.Status != CheckInStatus.Submitted)
            {
                TempData["Error"] = "The guest has not submitted their check-in for review.";
                return RedirectToAction(nameof(Review), new { id = booking.Id });
            }

            var doc = checkIn.Documents.First(d => d.Id == documentId);
            doc.Status = verify ? DocumentStatus.Verified : DocumentStatus.Rejected;

            if (!verify)
            {
                // Send it back so the guest can upload a new file
                checkIn.Status = CheckInStatus.InProgress;

                await _notifier.NotifyAsync(booking.GuestId, "Check-in document rejected",
                    $"Your {doc.DocumentType} for '{booking.Property.Title}' was rejected. Please upload it again.",
                    NotificationType.General);
            }
            else
            {
                var config = CheckInHelper.Read(checkIn.CheckInProcess);
                bool allVerified = config.Documents.All(type =>
                    checkIn.Documents.Any(d => d.DocumentType == type && d.Status == DocumentStatus.Verified));

                if (allVerified)
                {
                    checkIn.Status = CheckInStatus.Verified;
                    booking.Status = BookingStatus.CheckedIn;

                    await _notifier.NotifyAsync(booking.GuestId, "Check-in complete",
                        $"You are checked in for '{booking.Property.Title}'. Enjoy your stay!",
                        NotificationType.General);
                }
            }

            await _db.SaveChangesAsync();

            await _logger.LogAsync(CurrentUserId,
                $"{(verify ? "Verified" : "Rejected")} {doc.DocumentType} for booking #{booking.Id}",
                ActivityType.BookingStatus);

            if (booking.Status == BookingStatus.CheckedIn)
                TempData["Success"] = "All documents verified. The guest is now checked in.";
            else
                TempData["Success"] = verify ? "Document verified." : "Document rejected. The guest was asked to upload it again.";

            return RedirectToAction(nameof(Review), new { id = booking.Id });
        }
    }
}
