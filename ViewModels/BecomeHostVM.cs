using System.ComponentModel.DataAnnotations;

namespace Staybnb.ViewModels
{
    public class BecomeHostVM
    {
        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(1, 1000000)]
        [Display(Name = "Price per night (R)")]
        public decimal PricePerNight { get; set; }

        [Range(0, 1000000)]
        [Display(Name = "Cleaning fee (R)")]
        public decimal CleaningFee { get; set; }

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Property type")]
        public string PropertyType { get; set; } = string.Empty;

        [Range(1, 50)]
        [Display(Name = "Max guests")]
        public int MaxGuests { get; set; } = 1;

        [Range(0, 50)]
        public int Bedrooms { get; set; } = 1;

        [Range(0, 50)]
        public int Beds { get; set; } = 1;

        [Range(0, 50)]
        public int Bathrooms { get; set; } = 1;

        public List<IFormFile> Images { get; set; } = new();

        public List<int> AmenityIds { get; set; } = new();
    }
}
