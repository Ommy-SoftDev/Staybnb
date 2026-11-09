using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Enums;
using Staybnb.Models;
using Staybnb.Services;

namespace Staybnb.Controllers
{
    [Authorize(Roles = "Guest")]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ActivityLogger _logger;
        private readonly NotificationService _notifier;

        public BookingsController(ApplicationDbContext db, ActivityLogger logger, NotificationService notifier)
        {
            _db = db;
            _logger = logger;
            _notifier = notifier;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ---------- Request a booking ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int propertyId, DateTime checkIn, DateTime checkOut, int guests)
        {
            var property = await _db.HostProperties
                .FirstOrDefaultAsync(p => p.Id == propertyId && p.IsActive);
            if (property == null) return NotFound();

            checkIn = checkIn.Date;
            checkOut = checkOut.Date;

            string? error = null;

            if (checkIn < DateTime.Today)
            {
                error = "Check-in cannot be in the past.";
            }
            else if (checkOut <= checkIn)
            {
                error = "Check-out must be after check-in.";
            }
            else if (guests < 1 || guests > property.MaxGuests)
            {
                error = $"This property takes between 1 and {property.MaxGuests} guests.";
            }
            else
            {
                bool taken = await _db.Bookings.AnyAsync(b =>
                    b.PropertyId == propertyId &&
                    (b.Status == BookingStatus.Pending ||
                     b.Status == BookingStatus.Approved ||
                     b.Status == BookingStatus.CheckedIn) &&
                    checkIn < b.CheckOutDate && checkOut > b.CheckInDate);

                if (taken) error = "Those dates are not available. Please choose different dates.";
            }

            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("Details", "Properties", new { id = propertyId });
            }

            int nights = (checkOut - checkIn).Days;
            decimal total = BookingPricing.Total(property.PricePerNight, nights, property.CleaningFee);

            var booking = new Booking
            {
                PropertyId = propertyId,
                GuestId = CurrentUserId,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                NumberOfGuests = guests,
                TotalPrice = total,
                Status = BookingStatus.Pending,
                Payment = new Payment
                {
                    Amount = total,
                    Status = PaymentStatus.Pending
                }
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            await _logger.LogAsync(CurrentUserId,
                $"Requested booking #{booking.Id} for '{property.Title}' ({nights} nights)",
                ActivityType.BookingStatus);

            await _notifier.NotifyAsync(CurrentUserId, "Booking request sent",
                $"Your request for '{property.Title}' is waiting for the host.",
                NotificationType.General);

            await _notifier.NotifyAsync(property.HostId, "New booking request",
                $"A guest asked to book '{property.Title}' from {checkIn:yyyy-MM-dd} to {checkOut:yyyy-MM-dd}.",
                NotificationType.General);

            TempData["Success"] = "Booking request sent. The host will approve or reject it.";
            return RedirectToAction(nameof(MyBookings));
        }

        // ---------- The guest's bookings ----------
        public async Task<IActionResult> MyBookings()
        {
            var bookings = await _db.Bookings
                .Include(b => b.Property).ThenInclude(p => p.Images)
                .Include(b => b.GuestCheckIn)
                .Where(b => b.GuestId == CurrentUserId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _db.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b => b.Id == id && b.GuestId == CurrentUserId);
            if (booking == null) return NotFound();

            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Approved)
            {
                TempData["Error"] = "This booking can no longer be cancelled.";
                return RedirectToAction(nameof(MyBookings));
            }

            booking.Status = BookingStatus.Cancelled;
            await _db.SaveChangesAsync();

            await _logger.LogAsync(CurrentUserId,
                $"Cancelled booking #{booking.Id} for '{booking.Property.Title}'",
                ActivityType.BookingStatus);

            await _notifier.NotifyAsync(booking.Property.HostId, "Booking cancelled",
                $"A guest cancelled their booking for '{booking.Property.Title}'.",
                NotificationType.General);

            TempData["Success"] = "Booking cancelled.";
            return RedirectToAction(nameof(MyBookings));
        }
    }
}
