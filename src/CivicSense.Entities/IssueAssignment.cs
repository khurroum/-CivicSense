
using System;

namespace CivicSense.Entities
{
    public class IssueAssignment
    {
        public int Id { get; set; }

        public int IssueId { get; set; }
        public virtual Issue Issue { get; set; } = null!;

        public int StaffId { get; set; }
        public virtual User Staff { get; set; } = null!;

        public int AssignedById { get; set; }
        public virtual User AssignedBy { get; set; } = null!;

        public DateTime AssignedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? UnassignedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public string? AssignmentNote { get; set; }

        public DateTime? ExpectedCompletionAt { get; set; }
    }
}
