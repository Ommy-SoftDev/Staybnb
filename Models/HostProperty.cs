using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Staybnb.Models
{
    public class HostProperty
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerNight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CleaningFee { get; set; }

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string PropertyType { get; set; } = string.Empty;

        public int MaxGuests { get; set; } = 1;
        public int Bedrooms { get; set; } = 1;
        public int Beds { get; set; } = 1;
        public int Bathrooms { get; set; } = 1;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string HostId { get; set; } = string.Empty;
        public ApplicationUser Host { get; set; } = null!;

        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
        public CheckInProcess? CheckInProcess { get; set; }
    }
}
