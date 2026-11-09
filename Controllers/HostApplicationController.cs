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
    [Authorize(Roles = "Guest")]
    public class HostApplicationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ActivityLogger _logger;
        private readonly NotificationService _notifier;
        private readonly UserManager<ApplicationUser> _userManager;

        public HostApplicationController(ApplicationDbContext db, IWebHostEnvironment env,
            ActivityLogger logger, NotificationService notifier, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _env = env;
            _logger = logger;
            _notifier = notifier;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Apply()
        {
            ViewBag.Amenities = await _db.Amenities.OrderBy(a => a.Name).ToListAsync();
            return View(new BecomeHostVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(BecomeHostVM vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Only real, non-empty image files count
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var images = vm.Images.Where(f => f.Length > 0).ToList();

            if (images.Count == 0)
                ModelState.AddModelError("Images", "Please upload at least one image.");

            foreach (var file in images)
            {
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowed.Contains(ext))
                    ModelState.AddModelError("Images", "Only JPG, PNG or WEBP images are allowed.");
                if (file.Length > 5 * 1024 * 1024)
                    ModelState.AddModelError("Images", "Each image must be under 5MB.");
            }

            bool hasPending = await _db.HostApplications.AnyAsync(a =>
                a.ApplicationUserId == userId && a.Status == ApplicationStatus.Pending);
            if (hasPending)
                ModelState.AddModelError(string.Empty, "You already have an application waiting for review.");

            if (!ModelState.IsValid)
            {
                ViewBag.Amenities = await _db.Amenities.OrderBy(a => a.Name).ToListAsync();
                return View(vm);
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

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
                HostId = userId,
                IsActive = false
            };

            foreach (var file in images)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                var path = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                property.Images.Add(new PropertyImage { ImageUrl = "/uploads/" + fileName });
            }

            var chosen = await _db.Amenities.Where(a => vm.AmenityIds.Contains(a.Id)).ToListAsync();
            foreach (var amenity in chosen)
                property.Amenities.Add(amenity);

            _db.HostApplications.Add(new HostApplication
            {
                ApplicationUserId = userId,
                Property = property,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.UtcNow
            });

            // One save: property + images + application together
            await _db.SaveChangesAsync();

            await _logger.LogAsync(userId,
                $"Submitted host application for '{property.Title}'", ActivityType.Application);

            await _notifier.NotifyAsync(userId, "Application received",
                $"Your application for '{property.Title}' is under review.",
                NotificationType.HostApplication);

            var admins = new List<ApplicationUser>();
            admins.AddRange(await _userManager.GetUsersInRoleAsync("Admin"));
            admins.AddRange(await _userManager.GetUsersInRoleAsync("SuperAdmin"));
            foreach (var admin in admins)
            {
                await _notifier.NotifyAsync(admin.Id, "New host application",
                    $"'{property.Title}' is waiting for review.",
                    NotificationType.HostApplication);
            }

            TempData["Success"] = "Application submitted. An admin will review it soon.";
            return RedirectToAction(nameof(Status));
        }

        public async Task<IActionResult> Status()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var apps = await _db.HostApplications
                .Include(a => a.Property)
                .Where(a => a.ApplicationUserId == userId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
            return View(apps);
        }
    }
}
