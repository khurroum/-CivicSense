
using System;

namespace CivicSense.Entities
{
    public class IssueComment
    {
        public int Id { get; set; }

        public int IssueId { get; set; }
        public virtual Issue Issue { get; set; } = null!;

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public string Content { get; set; } = string.Empty;

        public bool IsInternal { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
