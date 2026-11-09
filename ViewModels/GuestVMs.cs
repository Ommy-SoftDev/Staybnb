using Staybnb.Models;

namespace Staybnb.ViewModels
{
    public class GalleryVM
    {
        public List<HostProperty> Properties { get; set; } = new();
        public List<string> Cities { get; set; } = new();
        public List<string> Types { get; set; } = new();
        public string? SelectedCity { get; set; }
        public string? SelectedType { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public int? Guests { get; set; }
        public HashSet<int> SavedIds { get; set; } = new();
    }

    public class CheckInFlowVM
    {
        public Booking Booking { get; set; } = null!;
        public GuestCheckIn CheckIn { get; set; } = null!;
        public CheckInConfig Config { get; set; } = new();
    }

    public class HostReviewVM
    {
        public Booking Booking { get; set; } = null!;
        public CheckInConfig Config { get; set; } = new();
    }
}
