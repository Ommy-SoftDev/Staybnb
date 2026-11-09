using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Staybnb.Enums;
using Staybnb.Models;

namespace Staybnb.Data
{
    // Runs on startup after the roles and SuperAdmin exist.
    //  - Amenities are always added if missing.
    //  - Demo users and properties are only added to an empty marketplace.
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            var env = services.GetRequiredService<IWebHostEnvironment>();

            await SeedAmenitiesAsync(db);
            await SeedDemoDataAsync(db, userManager, env.WebRootPath);
        }

        private static async Task SeedAmenitiesAsync(ApplicationDbContext db)
        {
            var wanted = new[]
            {
                ("WiFi", "wifi"), ("Kitchen", "kitchen"), ("Washer", "washer"), ("Dryer", "dryer"),
                ("Air conditioning", "ac"), ("Heating", "heating"), ("Free parking", "parking"),
                ("Pool", "pool"), ("TV", "tv"), ("Balcony", "balcony"), ("Braai area", "braai"),
                ("Workspace", "workspace")
            };

            foreach (var (name, icon) in wanted)
            {
                if (!await db.Amenities.AnyAsync(a => a.Name == name))
                    db.Amenities.Add(new Amenity { Name = name, IconClass = icon });
            }

            await db.SaveChangesAsync();
        }

        private static async Task SeedDemoDataAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager, string webRoot)
        {
            // Never touch a marketplace that already has listings
            if (await db.HostProperties.AnyAsync()) return;

            var host = await EnsureUserAsync(userManager, "host@staybnb.com", "Host@12345", "Demo", "Host", "Host");
            var guest = await EnsureUserAsync(userManager, "guest@staybnb.com", "Guest@12345", "Demo", "Guest", "Guest");
            if (host == null || guest == null) return;

            var amenities = await db.Amenities.ToListAsync();

            var specs = new List<(string Title, string Description, decimal Price, decimal Cleaning,
                string Address, string City, string Type, int Guests, int Bedrooms, int Beds, int Bathrooms, string Image, string[] Amenities, string Review)>
            {
                ("Lovely City Apartment",
                 "A comfortable, hotel-style apartment close to the beachfront, restaurants and shops. Perfect for a weekend away.",
                 1320m, 250m, "14 Marine Parade", "Durban", "Apartment", 4, 2, 3, 1, "/uploads/demo-1.svg",
                 new[] { "WiFi", "Kitchen", "Air conditioning", "TV", "Free parking" },
                 "Spotless and in a great spot. Would stay again."),

                ("Cozy Garden Suite",
                 "A warm, wood-panelled suite that opens onto a private tropical garden. Quiet, comfortable and close to town.",
                 3100m, 400m, "7 Garden Route", "Knysna", "Cottage", 4, 2, 2, 1, "/uploads/demo-2.svg",
                 new[] { "WiFi", "Kitchen", "Heating", "Braai area", "Balcony" },
                 "So peaceful. The host was very helpful."),

                ("Modern Glass Villa",
                 "A spacious modern villa with floor-to-ceiling glass, open-plan living and sweeping views. Room for the whole family.",
                 4800m, 600m, "3 Ocean View Drive", "Cape Town", "Villa", 8, 4, 6, 3, "/uploads/demo-3.svg",
                 new[] { "WiFi", "Pool", "Kitchen", "Washer", "Free parking", "TV" },
                 "Stunning views and plenty of space.")
            };

            foreach (var s in specs)
            {
                var property = new HostProperty
                {
                    Title = s.Title,
                    Description = s.Description,
                    PricePerNight = s.Price,
                    CleaningFee = s.Cleaning,
                    Address = s.Address,
                    City = s.City,
                    PropertyType = s.Type,
                    MaxGuests = s.Guests,
                    Bedrooms = s.Bedrooms,
                    Beds = s.Beds,
                    Bathrooms = s.Bathrooms,
                    HostId = host.Id,
                    IsActive = true
                };

                // Photos: real ones from wwwroot/uploads (demo-1.jpg, demo-1-b.jpg, ...) when they exist.
                // A property with no real photo gets the illustrated placeholders instead.
                var baseName = s.Image.Replace(".svg", "");
                var suffixes = new[] { "", "-b", "-c" };

                var realPhotos = suffixes
                    .Select(x => FindRealPhoto(webRoot, baseName + x))
                    .Where(x => x != null)
                    .ToList();

                if (realPhotos.Count > 0)
                {
                    foreach (var url in realPhotos)
                        property.Images.Add(new PropertyImage { ImageUrl = url! });
                }
                else
                {
                    foreach (var x in suffixes)
                        property.Images.Add(new PropertyImage { ImageUrl = baseName + x + ".svg" });
                }

                foreach (var amenity in amenities.Where(a => s.Amenities.Contains(a.Name)))
                    property.Amenities.Add(amenity);

                property.Reviews.Add(new Review
                {
                    ReviewerId = guest.Id,
                    Rating = 5,
                    Comment = s.Review
                });

                db.HostApplications.Add(new HostApplication
                {
                    ApplicationUserId = host.Id,
                    Property = property,
                    Status = ApplicationStatus.Approved,
                    AppliedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync();
        }

        // Returns the URL of a real photo (.jpg, .jpeg, .png, .webp) if one exists, otherwise null
        private static string? FindRealPhoto(string webRoot, string urlWithoutExtension)
        {
            foreach (var ext in new[] { ".jpg", ".jpeg", ".png", ".webp" })
            {
                var path = Path.Combine(webRoot,
                    urlWithoutExtension.TrimStart('/').Replace('/', Path.DirectorySeparatorChar) + ext);

                if (File.Exists(path)) return urlWithoutExtension + ext;
            }

            return null;
        }

        private static async Task<ApplicationUser?> EnsureUserAsync(
            UserManager<ApplicationUser> userManager, string email, string password,
            string firstName, string lastName, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null) return user;

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded) return null;

            await userManager.AddToRoleAsync(user, role);
            return user;
        }
    }
}
