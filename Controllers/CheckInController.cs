using System.Security.Claims;
using System.Text.Json;
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
    // The guest's three-step check-in: rules, documents, summary and submit
    [Authorize(Roles = "Guest")]
    public class CheckInController : Controller
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };

        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ActivityLogger _logger;
        private readonly NotificationService _notifier;

        public CheckInController(ApplicationDbContext db, IWebHostEnvironment env,
            ActivityLogger logger, NotificationService notifier)
        {
            _db = db;
            _env = env;
            _logger = logger;
            _notifier = notifier;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ---------- Step 1: house rules ----------
        [HttpGet]
        public async Task<IActionResult> Rules(int bookingId)
        {
            var booking = await LoadBookingAsync(bookingId);
            if (booking == null) return NotFound();

            var blocked = BlockIfNotApproved(booking);
            if (blocked != null) return blocked;

            return View(await BuildFlowAsync(booking));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rules(int bookingId, bool accept)
        {
            var booking = await LoadBookingAsync(bookingId);
            if (booking == null) return NotFound();

            var blocked = BlockIfNotApproved(booking);
            if (blocked != null) return blocked;

            if (!accept)
            {
                TempData["Error"] = "Please accept the house rules to continue.";
                return RedirectToAction(nameof(Rules), new { bookingId });
            }

            var vm = await BuildFlowAsync(booking);
            if (vm.CheckIn.Status == CheckInStatus.NotStarted)
            {
                vm.CheckIn.Status = CheckInStatus.InProgress;
                await _db.SaveChangesAsync();

                await _logger.LogAsync(CurrentUserId,
                    $"Started check-in for booking #{booking.Id}", ActivityType.BookingStatus);
            }

            return RedirectToAction(nameof(Documents), new { bookingId });
        }

        // ---------- Step 2: upload documents ----------
        [HttpGet]
        public async Task<IActionResult> Documents(int bookingId)
        {
            var booking = await LoadBookingAsync(bookingId);
            if (booking == null) return NotFound();

            var blocked = BlockIfNotApproved(booking);
            if (blocked != null) return blocked;

            var vm = await BuildFlowAsync(booking);
            if (vm.CheckIn.Status == CheckInStatus.NotStarted)
                return RedirectToAction(nameof(Rules), new { bookingId });

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int bookingId, string documentType, IFormFile? file)
        {
            var booking = await LoadBookingAsync(bookingId);
            if (booking == null) return NotFound();

            var blocked = BlockIfNotApproved(booking);
            if (blocked != null) return blocked;

            var vm = await BuildFlowAsync(booking);

            string? error = null;

            if (vm.CheckIn.Status != CheckInStatus.InProgress)
                error = "You can only upload documents while your check-in is in progress.";
            else if (!vm.Config.Documents.Contains(documentType))
                error = "That document is not required for this stay.";
            else if (file == null || file.Length == 0)
                error = "Please choose a file to upload.";
            else if (!AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
                error = "Only JPG, PNG or PDF files are allowed.";
            else if (file.Length > 5 * 1024 * 1024)
                error = "The file must be under 5MB.";

            var existing = vm.CheckIn.Documents.FirstOrDefault(d => d.DocumentType == documentType);
            if (error == null && existing != null && existing.Status == DocumentStatus.Verified)
                error = "That document has already been verified.";

            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Documents), new { bookingId });
            }

            // Documents are private, so they go outside wwwroot
            var folder = DocumentFolder();
            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file!.FileName).ToLower();
            using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (existing != null)
            {
                var oldPath = Path.Combine(folder, existing.FileName);
                _db.GuestDocuments.Remove(existing);
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            _db.GuestDocuments.Add(new GuestDocument
            {
                GuestCheckInId = vm.CheckIn.Id,
                DocumentType = documentType,
                FileName = fileName,
                Status = DocumentStatus.Pending
            });
            await _db.SaveChangesAsync();

            await _logger.LogAsync(CurrentUserId,
                $"Uploaded {documentType} for booking #{booking.Id}", ActivityType.BookingStatus);

            TempData["Success"] = $"{documentType} uploaded.";
            return RedirectToAction(nameof(Documents), new { bookingId });
        }

        // ---------- Step 3: summary and submit ----------
        [HttpGet]
        public async Task<IActionResult> Summary(int bookingId)
        {
            var booking = await LoadBookingAsync(bookingId);
            if (booking == null) return NotFound();

            var blocked = BlockIfNotApproved(booking);
            if (blocked != null) return blocked;

            var vm = await BuildFlowAsync(booking);
            if (vm.CheckIn.Status == CheckInStatus.NotStarted)
                return RedirectToAction(nameof(Rules), new { bookingId });

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int bookingId)
        {
            var booking = await LoadBookingAsync(bookingId);
            if (booking == null) return NotFound();

            var blocked = BlockIfNotApproved(booking);
            if (blocked != null) return blocked;

            var vm = await BuildFlowAsync(booking);

            if (vm.CheckIn.Status != CheckInStatus.InProgress)
            {
                TempData["Error"] = "This check-in cannot be submitted right now.";
                return RedirectToAction("MyBookings", "Bookings");
            }

            var missing = vm.Config.Documents
                .Where(type => !vm.CheckIn.Documents.Any(d =>
                    d.DocumentType == type && d.Status != DocumentStatus.Rejected))
                .ToList();

            if (missing.Any())
            {
                TempData["Error"] = "Please upload: " + string.Join(", ", missing);
                return RedirectToAction(nameof(Documents), new { bookingId });
            }

            vm.CheckIn.Status = CheckInStatus.Submitted;
            await _db.SaveChangesAsync();

            await _logger.LogAsync(CurrentUserId,
                $"Submitted check-in for booking #{booking.Id}", ActivityType.BookingStatus);

            await _notifier.NotifyAsync(booking.Property.HostId, "Check-in submitted",
                $"A guest submitted their check-in documents for '{booking.Property.Title}'.",
                NotificationType.General);

            TempData["Success"] = "Check-in submitted. The host will verify your documents.";
            return RedirectToAction("MyBookings", "Bookings");
        }

        // ---------- helpers ----------
        private string DocumentFolder()
        {
            return Path.Combine(_env.ContentRootPath, "App_Data", "guest-documents");
        }

        private async Task<Booking?> LoadBookingAsync(int bookingId)
        {
            return await _db.Bookings
                .Include(b => b.Property).ThenInclude(p => p.CheckInProcess)
                .Include(b => b.GuestCheckIn).ThenInclude(c => c!.Documents)
                .AsSplitQuery()
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.GuestId == CurrentUserId);
        }

        private IActionResult? BlockIfNotApproved(Booking booking)
        {
            if (booking.Status == BookingStatus.Approved) return null;

            TempData["Error"] = "Check-in is only available for approved bookings.";
            return RedirectToAction("MyBookings", "Bookings");
        }

        private async Task<CheckInFlowVM> BuildFlowAsync(Booking booking)
        {
            var process = booking.Property.CheckInProcess;

            // If the host never set up a check-in process, create a simple default one
            if (process == null)
            {
                var defaults = new CheckInConfig();
                defaults.Documents.Add("ID Document");

                process = new CheckInProcess
                {
                    PropertyId = booking.PropertyId,
                    Title = $"Check-in for {booking.Property.Title}",
                    StepsJson = JsonSerializer.Serialize(defaults)
                };
                _db.CheckInProcesses.Add(process);
                await _db.SaveChangesAsync();
                booking.Property.CheckInProcess = process;
            }

            if (booking.GuestCheckIn == null)
            {
                var checkIn = new GuestCheckIn
                {
                    BookingId = booking.Id,
                    CheckInProcessId = process.Id,
                    Status = CheckInStatus.NotStarted
                };
                _db.GuestCheckIns.Add(checkIn);
                await _db.SaveChangesAsync();
                booking.GuestCheckIn = checkIn;
            }

            return new CheckInFlowVM
            {
                Booking = booking,
                CheckIn = booking.GuestCheckIn,
                Config = CheckInHelper.Read(process)
            };
        }
    }
}
