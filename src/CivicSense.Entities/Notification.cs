
using System;

namespace CivicSense.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int? IssueId { get; set; }
        public virtual Issue? Issue { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string NotificationType { get; set; }
            = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }
    }
}
