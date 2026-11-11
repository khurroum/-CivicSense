
using CivicSense.Entities;

namespace CivicSense.Facade.DTOs
{
    // Admin: issue waiting for administrative action.
    public class WorkflowQueueItemDto
    {
        public int IssueId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string ReporterName { get; set; } = string.Empty;

        public IssueStatus Status { get; set; }

        public IssuePriority Priority { get; set; }

        public DateTime ReportedAt { get; set; }
    }

    // Staff: issue currently assigned to them.
    public class WorkflowAssignmentDto
    {
        public int AssignmentId { get; set; }

        public int IssueId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public IssueStatus Status { get; set; }

        public IssuePriority Priority { get; set; }

        public DateTime AssignedAt { get; set; }

        public DateTime? ExpectedCompletionAt { get; set; }

        public string? AssignmentNote { get; set; }
    }

    // Admin: option for selecting an active Staff member.
    public class WorkflowStaffOptionDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
    }

    // Admin or assigned Staff: status change record.
    public class WorkflowHistoryDto
    {
        public int Id { get; set; }

        public int IssueId { get; set; }

        public IssueStatus? OldStatus { get; set; }

        public IssueStatus NewStatus { get; set; }

        public string ChangedByName { get; set; }
            = string.Empty;

        public string? Note { get; set; }

        public DateTime ChangedAt { get; set; }
    }
}
