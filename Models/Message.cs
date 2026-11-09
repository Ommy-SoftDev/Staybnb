namespace Staybnb.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser Sender { get; set; } = null!;

        public string ReceiverId { get; set; } = string.Empty;
        public ApplicationUser Receiver { get; set; } = null!;

        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
    }
}