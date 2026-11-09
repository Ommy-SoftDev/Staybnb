using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Staybnb.Models;

namespace Staybnb.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<HostProperty> HostProperties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<HostApplication> HostApplications { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<CheckInProcess> CheckInProcesses { get; set; }
        public DbSet<GuestCheckIn> GuestCheckIns { get; set; }
        public DbSet<GuestDocument> GuestDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // One-to-one relationships
            b.Entity<Booking>()
                .HasOne(x => x.Payment).WithOne(p => p.Booking)
                .HasForeignKey<Payment>(p => p.BookingId);

            b.Entity<Booking>()
                .HasOne(x => x.GuestCheckIn).WithOne(g => g.Booking)
                .HasForeignKey<GuestCheckIn>(g => g.BookingId);

            b.Entity<HostProperty>()
                .HasOne(p => p.CheckInProcess).WithOne(c => c.Property)
                .HasForeignKey<CheckInProcess>(c => c.PropertyId);

            // Stops SQL Server "multiple cascade paths" errors
            b.Entity<HostProperty>()
                .HasOne(p => p.Host).WithMany()
                .HasForeignKey(p => p.HostId).OnDelete(DeleteBehavior.Restrict);

            b.Entity<Booking>()
                .HasOne(x => x.Guest).WithMany()
                .HasForeignKey(x => x.GuestId).OnDelete(DeleteBehavior.Restrict);

            b.Entity<Review>()
                .HasOne(x => x.Reviewer).WithMany()
                .HasForeignKey(x => x.ReviewerId).OnDelete(DeleteBehavior.Restrict);

            b.Entity<WishlistItem>()
                .HasOne(x => x.User).WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

            b.Entity<HostApplication>()
                .HasOne(x => x.ApplicationUser).WithMany()
                .HasForeignKey(x => x.ApplicationUserId).OnDelete(DeleteBehavior.Restrict);

            b.Entity<Message>()
                .HasOne(m => m.Sender).WithMany()
                .HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);

            b.Entity<Message>()
                .HasOne(m => m.Receiver).WithMany()
                .HasForeignKey(m => m.ReceiverId).OnDelete(DeleteBehavior.Restrict);

            b.Entity<GuestCheckIn>()
                .HasOne(g => g.CheckInProcess).WithMany()
                .HasForeignKey(g => g.CheckInProcessId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}