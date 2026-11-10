
using System;

namespace CivicSense.Entities
{
    public class Feedback
    {
        public int Id { get; set; }

        public int IssueId { get; set; }
        public virtual Issue Issue { get; set; } = null!;

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public bool IsResolutionConfirmed { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}
