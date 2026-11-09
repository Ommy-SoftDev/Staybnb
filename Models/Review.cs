using System.ComponentModel.DataAnnotations;

namespace Staybnb.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int PropertyId { get; set; }
        public HostProperty Property { get; set; } = null!;

        public string ReviewerId { get; set; } = string.Empty;
        public ApplicationUser Reviewer { get; set; } = null!;

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}