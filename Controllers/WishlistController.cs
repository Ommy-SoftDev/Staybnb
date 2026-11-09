using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;

namespace Staybnb.Controllers
{
    [Authorize(Roles = "Guest")]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _db;

        public WishlistController(ApplicationDbContext db)
        {
            _db = db;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            var ids = await _db.WishlistItems
                .Where(w => w.UserId == CurrentUserId)
                .Select(w => w.PropertyId)
                .ToListAsync();

            var properties = await _db.HostProperties
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Where(p => ids.Contains(p.Id) && p.IsActive)
                .ToListAsync();

            ViewBag.SavedIds = ids.ToHashSet();
            return View(properties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int propertyId, string? returnUrl)
        {
            var existing = await _db.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == CurrentUserId && w.PropertyId == propertyId);

            if (existing != null)
            {
                _db.WishlistItems.Remove(existing);
            }
            else
            {
                bool propertyExists = await _db.HostProperties.AnyAsync(p => p.Id == propertyId && p.IsActive);
                if (!propertyExists) return NotFound();

                _db.WishlistItems.Add(new WishlistItem
                {
                    UserId = CurrentUserId,
                    PropertyId = propertyId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }
    }
}
