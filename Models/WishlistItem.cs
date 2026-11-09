namespace Staybnb.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int PropertyId { get; set; }
        public HostProperty Property { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}