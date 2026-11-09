using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Staybnb.Models;

namespace Staybnb.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Creates the database automatically if it doesn't exist yet
            await db.Database.MigrateAsync();

            // 1. Create the four roles
            string[] roles = { "SuperAdmin", "Admin", "Host", "Guest" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2. Create the SuperAdmin user
            var email = "superadmin@staybnb.com";
            var superAdmin = await userManager.FindByEmailAsync(email);
            if (superAdmin == null)
            {
                superAdmin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = "Super",
                    LastName = "Admin"
                };

                var result = await userManager.CreateAsync(superAdmin, "Admin@12345");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
            }
        }
    }
}