using System.ComponentModel.DataAnnotations;
using Staybnb.Enums;
using Staybnb.Models;

namespace Staybnb.ViewModels
{
    public class HostDashboardVM
    {
        public string HostName { get; set; } = string.Empty;
        public int TotalProperties { get; set; }
        public int ActiveProperties { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public decimal TotalEarnings { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        public int BookingsThisMonth { get; set; }
        public decimal EarningsThisMonth { get; set; }
        public double OccupancyRate { get; set; }
        public double AcceptanceRate { get; set; }
        public decimal AverageEarningsPerBooking { get; set; }
        public decimal AverageNightlyRate { get; set; }
    }

    public class MonthEarningVM
    {
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class HostEarningsVM
    {
        public decimal Total { get; set; }
        public decimal ThisMonth { get; set; }
        public int PaidBookings { get; set; }
        public List<MonthEarningVM> Months { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
    }

    public class HostPropertyRowVM
    {
        public HostProperty Property { get; set; } = null!;
        public ApplicationStatus? ApplicationStatus { get; set; }
    }

    public class EditPropertyVM
    {
        public int Id { get; set; }

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

        public List<IFormFile> NewImages { get; set; } = new();
        public List<int> AmenityIds { get; set; } = new();
        public List<PropertyImage> ExistingImages { get; set; } = new();
    }

    public class CheckInSetupVM
    {
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;

        [Required, StringLength(200)]
        [Display(Name = "Check-in title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "House rules and steps (one per line)")]
        public string RulesText { get; set; } = string.Empty;

        [Display(Name = "ID document")]
        public bool RequireId { get; set; }

        [Display(Name = "Passport")]
        public bool RequirePassport { get; set; }

        [Display(Name = "Proof of address")]
        public bool RequireProofOfAddress { get; set; }
    }

    // What gets stored in CheckInProcess.StepsJson
    public class CheckInConfig
    {
        public List<string> Rules { get; set; } = new();
        public List<string> Documents { get; set; } = new();
    }
}
