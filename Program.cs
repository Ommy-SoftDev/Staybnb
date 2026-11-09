using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;
using Staybnb.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity with roles
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Where to send people who are logged out or not allowed
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Role changes take effect straight away (user must log in again)
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});

// Our own services
builder.Services.AddScoped<ActivityLogger>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Create the database, roles, SuperAdmin, amenities and demo data on startup
using (var scope = app.Services.CreateScope())
{
    await RoleSeeder.SeedAsync(scope.ServiceProvider);

    try
    {
        await DataSeeder.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        log.LogError(ex, "Seeding sample data failed. The app will keep running.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();