using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Enums;
using Staybnb.Models;
using Staybnb.ViewModels;

namespace Staybnb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        // The Airbnb-style gallery. Filters: city, property type, dates and guests.
        public async Task<IActionResult> Index(string? city, string? type, DateTime? checkIn, DateTime? checkOut, int? guests)
        {
            var query = _db.HostProperties
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(p => p.City.Contains(city));

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(p => p.PropertyType == type);

            if (guests.HasValue && guests.Value > 0)
                query = query.Where(p => p.MaxGuests >= guests.Value);

            // Only show places that are free for the chosen dates
            if (checkIn.HasValue && checkOut.HasValue && checkOut.Value.Date > checkIn.Value.Date)
            {
                var from = checkIn.Value.Date;
                var to = checkOut.Value.Date;

                query = query.Where(p => !p.Bookings.Any(b =>
                    (b.Status == BookingStatus.Pending ||
                     b.Status == BookingStatus.Approved ||
                     b.Status == BookingStatus.CheckedIn) &&
                    from < b.CheckOutDate && to > b.CheckInDate));
            }

            var vm = new GalleryVM
            {
                Properties = await query.OrderByDescending(p => p.CreatedAt).ToListAsync(),
                Types = await _db.HostProperties
                    .Where(p => p.IsActive)
                    .Select(p => p.PropertyType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync(),
                SelectedCity = city,
                SelectedType = type,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Guests = guests
            };

            if (User.IsInRole("Guest"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                vm.SavedIds = (await _db.WishlistItems
                    .Where(w => w.UserId == userId)
                    .Select(w => w.PropertyId)
                    .ToListAsync()).ToHashSet();
            }

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
