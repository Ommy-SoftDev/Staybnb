using Staybnb.Models;

namespace Staybnb.ViewModels
{
    public class AdminDashboardVM
    {
        public int TotalUsers { get; set; }
        public int PendingApplications { get; set; }
        public int ActiveProperties { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ApprovedHosts { get; set; }
        public decimal PlatformRevenue { get; set; }
        public List<HostApplication> RecentApplications { get; set; } = new();
        public List<ActivityLog> RecentActivity { get; set; } = new();
        public List<Booking> RecentBookings { get; set; } = new();
    }

    public class UserRowVM
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
