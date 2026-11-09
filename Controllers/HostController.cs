using System.Security.Claims;
using System.Text.Json;
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
    [Authorize(Roles = "Host")]
    public class HostController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ActivityLogger _logger;
        private readonly NotificationService _notifier;

        public HostController(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            ActivityLogger logger,
            NotificationService notifier)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
            _logger = logger;
            _notifier = notifier;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ---------- Dashboard ----------
        public async Task<IActionResult> Dashboard()
        {
            var userId = CurrentUserId;
            var user = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var ratings = await _db.Reviews
                .Where(r => r.Property.HostId == userId)
                .Select(r => r.Rating)
                .ToListAsync();

            var bookings = await _db.Bookings
                .Include(b => b.Payment)
                .Where(b => b.Property.HostId == userId)
                .ToListAsync();

            var prices = await _db.HostProperties
                .Where(p => p.HostId == userId)
                .Select(p => p.PricePerNight)
                .ToListAsync();

            int activeCount = await _db.HostProperties.CountAsync(p => p.HostId == userId && p.IsActive);

            var paid = bookings
                .Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Paid)
                .ToList();
            decimal totalEarnings = paid.Sum(b => b.Payment!.Amount);
            decimal earningsThisMonth = paid
                .Where(b => b.Payment!.CreatedAt >= monthStart)
                .Sum(b => b.Payment!.Amount);

            // Occupancy over the last 30 days
            var windowStart = today.AddDays(-30);
            int bookedNights = bookings
                .Where(b => b.Status == BookingStatus.Approved || b.Status == BookingStatus.CheckedIn)
                .Sum(b =>
                {
                    var start = b.CheckInDate > windowStart ? b.CheckInDate : windowStart;
                    var end = b.CheckOutDate < today ? b.CheckOutDate : today;
                    return Math.Max(0, (end - start).Days);
                });
            double occupancy = activeCount == 0 ? 0 : Math.Min(100, bookedNights * 100.0 / (activeCount * 30));

            int accepted = bookings.Count(b => b.Status == BookingStatus.Approved || b.Status == BookingStatus.CheckedIn);
            int rejected = bookings.Count(b => b.Status == BookingStatus.Rejected);
            double acceptance = accepted + rejected == 0 ? 0 : accepted * 100.0 / (accepted + rejected);

            var vm = new HostDashboardVM
            {
                HostName = NameHelper.Display(user),
                TotalProperties = await _db.HostProperties.CountAsync(p => p.HostId == userId),
                ActiveProperties = activeCount,
                TotalBookings = bookings.Count,
                PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending),
                BookingsThisMonth = bookings.Count(b => b.CreatedAt >= monthStart),
                TotalEarnings = totalEarnings,
                EarningsThisMonth = earningsThisMonth,
                ReviewCount = ratings.Count,
                AverageRating = ratings.Count == 0 ? 0 : ratings.Average(),
                OccupancyRate = occupancy,
                AcceptanceRate = acceptance,
                AverageEarningsPerBooking = paid.Count == 0 ? 0 : totalEarnings / paid.Count,
                AverageNightlyRate = prices.Count == 0 ? 0 : prices.Average()
            };

            return View(vm);
        }

        // ---------- Earnings ----------
        public async Task<IActionResult> Earnings()
        {
            var userId = CurrentUserId;
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var payments = await _db.Payments
                .Include(p => p.Booking).ThenInclude(b => b.Property)
                .Include(p => p.Booking).ThenInclude(b => b.Guest)
                .Where(p => p.Status == PaymentStatus.Paid && p.Booking.Property.HostId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var months = Enumerable.Range(0, 6)
                .Select(i => today.AddMonths(-i))
                .Select(d => new MonthEarningVM
                {
                    Label = d.ToString("MMM yyyy"),
                    Amount = payments
                        .Where(p => p.CreatedAt.Year == d.Year && p.CreatedAt.Month == d.Month)
                        .Sum(p => p.Amount)
                })
                .ToList();

            return View(new HostEarningsVM
            {
                Total = payments.Sum(p => p.Amount),
                ThisMonth = payments.Where(p => p.CreatedAt >= monthStart).Sum(p => p.Amount),
                PaidBookings = payments.Count,
                Months = months,
                Payments = payments
            });
        }

        // ---------- Reviews ----------
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _db.Reviews
                .Include(r => r.Property)
                .Include(r => r.Reviewer)
                .Where(r => r.Property.HostId == CurrentUserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        // ---------- My properties ----------
        public async Task<IActionResult> MyProperties()
        {
            var userId = CurrentUserId;

            var props = await _db.HostProperties
                .Include(p => p.Images)
                .Include(p => p.Bookings)
                .Include(p => p.Reviews)
                .Where(p => p.HostId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var ids = props.Select(p => p.Id).ToList();
            var apps = await _db.HostApplications
                .Where(a => ids.Contains(a.PropertyId))
                .ToListAsync();

            var rows = props.Select(p => new HostPropertyRowVM
            {
                Property = p,
                ApplicationStatus = apps
                    .Where(a => a.PropertyId == p.Id)
                    .OrderByDescending(a => a.AppliedAt)
                    .Select(a => (ApplicationStatus?)a.Status)
                    .FirstOrDefault()
            }).ToList();

            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var property = await _db.HostProperties
                .FirstOrDefaultAsync(p => p.Id == id && p.HostId == CurrentUserId);
            if (property == null) return NotFound();

            bool approved = await _db.HostApplications.AnyAsync(a =>
                a.PropertyId == id && a.Status == ApplicationStatus.Approved);

            if (!approved)
            {
                TempData["Error"] = "A property can only be switched on after an admin approves it.";
                return RedirectToAction(nameof(MyProperties));
            }

            property.IsActive = !property.IsActive;
            await _db.SaveChangesAsync();

            var state = property.IsActive ? "Activated" : "Deactivated";
            await _logger.LogAsync(CurrentUserId, $"{state} property '{property.Title}'", ActivityType.Other);

            TempData["Success"] = $"'{property.Title}' is now {(property.IsActive ? "active" : "inactive")}.";
            return RedirectToAction(nameof(MyProperties));
        }

        // ---------- Add another property (needs admin approval) ----------
        [HttpGet]
        public async Task<IActionResult> AddProperty()
        {
            await LoadAmenitiesAsync();
            return View(new BecomeHostVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProperty(BecomeHostVM vm)
        {
            var images = vm.Images.Where(f => f.Length > 0).ToList();
            ValidateImages(images, "Images", required: true);

            if (!ModelState.IsValid)
            {
                await LoadAmenitiesAsync();
                return View(vm);
            }

            var property = new HostProperty
            {
                Title = vm.Title,
                Description = vm.Description,
                PricePerNight = vm.PricePerNight,
                CleaningFee = vm.CleaningFee,
                Address = vm.Address,
                City = vm.City,
                PropertyType = vm.PropertyType,
                MaxGuests = vm.MaxGuests,
                Bedrooms = vm.Bedrooms,
                Beds = vm.Beds,
                Bathrooms = vm.Bathrooms,
                HostId = CurrentUserId,
                IsActive = false
            };

            foreach (var img in await SaveImagesAsync(images))
                property.Images.Add(img);

            var chosen = await _db.Amenities.Where(a => vm.AmenityIds.Contains(a.Id)).ToListAsync();
            foreach (var amenity in chosen)
                property.Amenities.Add(amenity);

            _db.HostApplications.Add(new HostApplication
            {
                ApplicationUserId = CurrentUserId,
                Property = property,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            await _logger.LogAsync(CurrentUserId,
                $"Submitted additional property application for '{property.Title}'", ActivityType.Application);

            await _notifier.NotifyAsync(CurrentUserId, "Additional property application submitted",
                $"Your application for '{property.Title}' is under review.",
                NotificationType.HostApplication);

            var admins = new List<ApplicationUser>();
            admins.AddRange(await _userManager.GetUsersInRoleAsync("Admin"));
            admins.AddRange(await _userManager.GetUsersInRoleAsync("SuperAdmin"));
            foreach (var admin in admins)
            {
                await _notifier.NotifyAsync(admin.Id, "New property application",
                    $"'{property.Title}' is waiting for review.",
                    NotificationType.HostApplication);
            }

            TempData["Success"] = "Property submitted. An admin will review it before it goes live.";
            return RedirectToAction(nameof(MyProperties));
        }

        // ---------- Edit a property ----------
        [HttpGet]
        public async Task<IActionResult> EditProperty(int id)
        {
            var p = await _db.HostProperties
                .Include(x => x.Images)
                .Include(x => x.Amenities)
                .FirstOrDefaultAsync(x => x.Id == id && x.HostId == CurrentUserId);
            if (p == null) return NotFound();

            await LoadAmenitiesAsync();

            return View(new EditPropertyVM
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                PricePerNight = p.PricePerNight,
                CleaningFee = p.CleaningFee,
                Address = p.Address,
                City = p.City,
                PropertyType = p.PropertyType,
                MaxGuests = p.MaxGuests,
                Bedrooms = p.Bedrooms,
                Beds = p.Beds,
                Bathrooms = p.Bathrooms,
                ExistingImages = p.Images.ToList(),
                AmenityIds = p.Amenities.Select(a => a.Id).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(EditPropertyVM vm)
        {
            var p = await _db.HostProperties
                .Include(x => x.Images)
                .Include(x => x.Amenities)
                .FirstOrDefaultAsync(x => x.Id == vm.Id && x.HostId == CurrentUserId);
            if (p == null) return NotFound();

            var newImages = vm.NewImages.Where(f => f.Length > 0).ToList();
            ValidateImages(newImages, "NewImages", required: false);

            if (!ModelState.IsValid)
            {
                vm.ExistingImages = p.Images.ToList();
                await LoadAmenitiesAsync();
                return View(vm);
            }

            p.Title = vm.Title;
            p.Description = vm.Description;
            p.PricePerNight = vm.PricePerNight;
            p.CleaningFee = vm.CleaningFee;
            p.Address = vm.Address;
            p.City = vm.City;
            p.PropertyType = vm.PropertyType;
            p.MaxGuests = vm.MaxGuests;
            p.Bedrooms = vm.Bedrooms;
            p.Beds = vm.Beds;
            p.Bathrooms = vm.Bathrooms;

            p.Amenities.Clear();
            var chosenAmenities = await _db.Amenities.Where(a => vm.AmenityIds.Contains(a.Id)).ToListAsync();
            foreach (var amenity in chosenAmenities)
                p.Amenities.Add(amenity);

            foreach (var img in await SaveImagesAsync(newImages))
                p.Images.Add(img);

            await _db.SaveChangesAsync();

            await _logger.LogAsync(CurrentUserId, $"Edited property '{p.Title}'", ActivityType.Other);

            TempData["Success"] = "Property updated.";
            return RedirectToAction(nameof(MyProperties));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePhoto(int id)
        {
            var image = await _db.PropertyImages
                .Include(i => i.Property)
                .FirstOrDefaultAsync(i => i.Id == id && i.Property.HostId == CurrentUserId);
            if (image == null) return NotFound();

            var propertyId = image.PropertyId;
            int count = await _db.PropertyImages.CountAsync(i => i.PropertyId == propertyId);

            if (count <= 1)
            {
                TempData["Error"] = "A property needs at least one photo. Add a new photo before removing this one.";
                return RedirectToAction(nameof(EditProperty), new { id = propertyId });
            }

            var url = image.ImageUrl;
            _db.PropertyImages.Remove(image);
            await _db.SaveChangesAsync();

            if (url.StartsWith("/uploads/"))
            {
                var fullPath = Path.Combine(_env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            }

            await _logger.LogAsync(CurrentUserId, "Removed a property photo", ActivityType.Other);

            TempData["Success"] = "Photo removed.";
            return RedirectToAction(nameof(EditProperty), new { id = propertyId });
        }

        // ---------- Check-in rules and required documents ----------
        [HttpGet]
        public async Task<IActionResult> CheckInSetup(int propertyId)
        {
            var p = await _db.HostProperties
                .Include(x => x.CheckInProcess)
                .FirstOrDefaultAsync(x => x.Id == propertyId && x.HostId == CurrentUserId);
            if (p == null) return NotFound();

            var vm = new CheckInSetupVM
            {
                PropertyId = p.Id,
                PropertyTitle = p.Title,
                Title = $"Check-in for {p.Title}"
            };

            if (p.CheckInProcess != null)
            {
                vm.Title = p.CheckInProcess.Title;
                try
                {
                    var config = JsonSerializer.Deserialize<CheckInConfig>(p.CheckInProcess.StepsJson);
                    if (config != null)
                    {
                        vm.RulesText = string.Join(Environment.NewLine, config.Rules);
                        vm.RequireId = config.Documents.Contains("ID Document");
                        vm.RequirePassport = config.Documents.Contains("Passport");
                        vm.RequireProofOfAddress = config.Documents.Contains("Proof of address");
                    }
                }
                catch (JsonException)
                {
                    // Old or broken data: show an empty form
                }
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckInSetup(CheckInSetupVM vm)
        {
            var p = await _db.HostProperties
                .Include(x => x.CheckInProcess)
                .FirstOrDefaultAsync(x => x.Id == vm.PropertyId && x.HostId == CurrentUserId);
            if (p == null) return NotFound();

            if (!vm.RequireId && !vm.RequirePassport && !vm.RequireProofOfAddress)
                ModelState.AddModelError(string.Empty, "Choose at least one document that guests must upload.");

            if (!ModelState.IsValid)
            {
                vm.PropertyTitle = p.Title;
                return View(vm);
            }

            var config = new CheckInConfig
            {
                Rules = (vm.RulesText ?? "")
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .Where(r => r.Length > 0)
                    .ToList()
            };
            if (vm.RequireId) config.Documents.Add("ID Document");
            if (vm.RequirePassport) config.Documents.Add("Passport");
            if (vm.RequireProofOfAddress) config.Documents.Add("Proof of address");

            var json = JsonSerializer.Serialize(config);

            if (p.CheckInProcess == null)
            {
                p.CheckInProcess = new CheckInProcess
                {
                    PropertyId = p.Id,
                    Title = vm.Title,
                    StepsJson = json
                };
            }
            else
            {
                p.CheckInProcess.Title = vm.Title;
                p.CheckInProcess.StepsJson = json;
            }

            await _db.SaveChangesAsync();

            await _logger.LogAsync(CurrentUserId,
                $"Updated check-in process for '{p.Title}'", ActivityType.Other);

            TempData["Success"] = "Check-in process saved.";
            return RedirectToAction(nameof(MyProperties));
        }

        // ---------- helpers ----------
        private async Task LoadAmenitiesAsync()
        {
            ViewBag.Amenities = await _db.Amenities.OrderBy(a => a.Name).ToListAsync();
        }

        private void ValidateImages(List<IFormFile> images, string key, bool required)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (required && images.Count == 0)
                ModelState.AddModelError(key, "Please upload at least one image.");

            foreach (var file in images)
            {
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowed.Contains(ext))
                    ModelState.AddModelError(key, "Only JPG, PNG or WEBP images are allowed.");
                if (file.Length > 5 * 1024 * 1024)
                    ModelState.AddModelError(key, "Each image must be under 5MB.");
            }
        }

        private async Task<List<PropertyImage>> SaveImagesAsync(List<IFormFile> files)
        {
            var folder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(folder);

            var saved = new List<PropertyImage>();
            foreach (var file in files)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                var path = Path.Combine(folder, fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                saved.Add(new PropertyImage { ImageUrl = "/uploads/" + fileName });
            }
            return saved;
        }
    }
}
