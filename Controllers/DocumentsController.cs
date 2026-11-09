using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;

namespace Staybnb.Controllers
{
    // Guest documents are private. Only the guest, the host of that property
    // and admins can open them.
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public DocumentsController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Download(int id)
        {
            var doc = await _db.GuestDocuments
                .Include(d => d.GuestCheckIn).ThenInclude(c => c.Booking).ThenInclude(b => b.Property)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (doc == null) return NotFound();

            var booking = doc.GuestCheckIn.Booking;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool allowed = booking.GuestId == userId
                           || booking.Property.HostId == userId
                           || User.IsInRole("Admin")
                           || User.IsInRole("SuperAdmin");
            if (!allowed) return Forbid();

            var path = Path.Combine(_env.ContentRootPath, "App_Data", "guest-documents", doc.FileName);
            if (!System.IO.File.Exists(path)) return NotFound();

            var ext = Path.GetExtension(path).ToLower();
            var contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                _ => "image/jpeg"
            };

            return PhysicalFile(path, contentType);
        }
    }
}
