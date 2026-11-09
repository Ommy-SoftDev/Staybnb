using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;

namespace Staybnb.Controllers
{
    // Anyone can look at a property. Booking needs a Guest login.
    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PropertiesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Details(int id)
        {
            var property = await _db.HostProperties
                .Include(p => p.Images)
                .Include(p => p.Host)
                .Include(p => p.Amenities)
                .Include(p => p.Reviews).ThenInclude(r => r.Reviewer)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (property == null) return NotFound();

            bool saved = false;
            if (User.IsInRole("Guest"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                saved = await _db.WishlistItems.AnyAsync(w => w.UserId == userId && w.PropertyId == id);
            }
            ViewBag.IsSaved = saved;

            return View(property);
        }
    }
}
