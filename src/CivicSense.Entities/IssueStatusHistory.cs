
using System;

namespace CivicSense.Entities
{
    public class IssueStatusHistory
    {
        public int Id { get; set; }

        public int IssueId { get; set; }
        public virtual Issue Issue { get; set; } = null!;

        public byte? OldStatus { get; set; }

        public byte NewStatus { get; set; }

        public int ChangedById { get; set; }
        public virtual User ChangedBy { get; set; } = null!;

        public string? Note { get; set; }

        public DateTime ChangedAt { get; set; }
            = DateTime.UtcNow;
    }
}
