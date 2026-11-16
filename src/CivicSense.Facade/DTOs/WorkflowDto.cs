using CivicSense.Entities;

namespace CivicSense.Facade.DTOs
{
    public class WorkflowQueueItemDto
    {
        public int IssueId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;
        public byte Status { get; set; }

        public string StatusName => IssueStatusLookup.NameOf(Status);
        public byte Priority { get; set; }

        public string PriorityName => IssuePriorityLookup.NameOf(Priority);
        public DateTime ReportedAt { get; set; }
    }

    public class WorkflowAssignmentDto
    {
        public int AssignmentId { get; set; }
        public int IssueId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public byte Status { get; set; }

        public string StatusName => IssueStatusLookup.NameOf(Status);
        public byte Priority { get; set; }

        public string PriorityName => IssuePriorityLookup.NameOf(Priority);
        public DateTime AssignedAt { get; set; }
        public DateTime? ExpectedCompletionAt { get; set; }
        public string? AssignmentNote { get; set; }
    }

    public class WorkflowStaffOptionDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class WorkflowHistoryDto
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public byte? OldStatus { get; set; }

        public string OldStatusName => OldStatus.HasValue ? IssueStatusLookup.NameOf(OldStatus) : "Initial Report";
        public byte NewStatus { get; set; }

        public string NewStatusName => IssueStatusLookup.NameOf(NewStatus);
        public string ChangedByName { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
