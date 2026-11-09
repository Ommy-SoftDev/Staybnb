using System.ComponentModel.DataAnnotations.Schema;
using Staybnb.Enums;

namespace Staybnb.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int PropertyId { get; set; }
        public HostProperty Property { get; set; } = null!;

        public string GuestId { get; set; } = string.Empty;
        public ApplicationUser Guest { get; set; } = null!;

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Payment? Payment { get; set; }
        public GuestCheckIn? GuestCheckIn { get; set; }
    }
}