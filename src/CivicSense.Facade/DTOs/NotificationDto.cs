
using System;

namespace CivicSense.Facade.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }

        public int? IssueId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string NotificationType { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ReadAt { get; set; }
    }
}
