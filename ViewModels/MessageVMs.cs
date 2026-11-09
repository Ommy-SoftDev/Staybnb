using Staybnb.Models;

namespace Staybnb.ViewModels
{
    public class ConversationRowVM
    {
        public string OtherUserId { get; set; } = string.Empty;
        public string OtherName { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastTime { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ConversationVM
    {
        public string CurrentUserId { get; set; } = string.Empty;
        public string OtherUserId { get; set; } = string.Empty;
        public string OtherName { get; set; } = string.Empty;
        public List<Message> Messages { get; set; } = new();
    }

    public class NotificationsVM
    {
        public List<Notification> Items { get; set; } = new();
        public Notification? Selected { get; set; }
        public string? ActionUrl { get; set; }
        public string Filter { get; set; } = "all";
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
    }
}
