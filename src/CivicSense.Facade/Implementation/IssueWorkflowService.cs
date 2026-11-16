
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.Interface;
using CivicSense.Facade.DTOs;

namespace CivicSense.Facade.Implementation
{
    public class IssueWorkflowService : IIssueWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly IIssueRepository _issues;
        private readonly IUserRepository _users;
        private readonly IIssueAssignmentRepository _assignments;
        private readonly IIssueStatusHistoryRepository _history;
        private readonly INotificationRepository _notifications;
        private readonly IIssueWorkflowQueryRepository _queries;

        public IssueWorkflowService(
            ApplicationDbContext context,
            IIssueRepository issues,
            IUserRepository users,
            IIssueAssignmentRepository assignments,
            IIssueStatusHistoryRepository history,
            INotificationRepository notifications,
            IIssueWorkflowQueryRepository queries)
        {
            _context = context;
            _issues = issues;
            _users = users;
            _assignments = assignments;
            _history = history;
            _notifications = notifications;
            _queries = queries;
        }

        public async Task<List<WorkflowQueueItemDto>> GetAdminQueueAsync(int adminId)
        {
            await FacadeValidation.RequireAdminAsync(_users, adminId);
            var issues = await _queries.GetAdminQueueAsync();
            return issues.Select(issue => new WorkflowQueueItemDto
            {
                IssueId = issue.Id,
                Title = issue.Title,
                CategoryName = issue.Category?.Name ?? "Unknown category",
                ReporterName = issue.Reporter?.FullName ?? "Unknown reporter",
                Status = issue.Status,
                Priority = issue.Priority,
                ReportedAt = issue.CreatedAt
            }).ToList();
        }

        public async Task<List<WorkflowStaffOptionDto>> GetActiveStaffAsync(int adminId)
        {
            await FacadeValidation.RequireAdminAsync(_users, adminId);
            var staff = await _queries.GetActiveStaffAsync();
            return staff.Select(user => new WorkflowStaffOptionDto
            {
                Id = user.Id,
                FullName = user.FullName
            }).ToList();
        }

        public async Task<List<WorkflowAssignmentDto>> GetMyAssignmentsAsync(int staffId)
        {
            var staff = await FacadeValidation.GetActiveUserAsync(_users, staffId);
            if (staff.Role != UserRoleLookup.Staff)
                throw new UnauthorizedAccessException("Staff access is required.");

            var assignments = await _queries.GetActiveAssignmentsForStaffAsync(staffId);
            return assignments.Select(assignment => new WorkflowAssignmentDto
            {
                AssignmentId = assignment.Id,
                IssueId = assignment.Issue.Id,
                Title = assignment.Issue.Title,
                Description = assignment.Issue.Description,
                Address = assignment.Issue.Address,
                CategoryName = assignment.Issue.Category?.Name ?? "Unknown category",
                Status = assignment.Issue.Status,
                Priority = assignment.Issue.Priority,
                AssignedAt = assignment.AssignedAt,
                ExpectedCompletionAt = assignment.ExpectedCompletionAt,
                AssignmentNote = assignment.AssignmentNote
            }).ToList();
        }

        public async Task<List<WorkflowHistoryDto>> GetHistoryAsync(int issueId, int actorId)
        {
            var user = await FacadeValidation.GetActiveUserAsync(_users, actorId);
            var issue = await GetCivicIssueAsync(issueId);
            if (user.Role != UserRoleLookup.Admin)
            {
                if (user.Role != UserRoleLookup.Staff)
                    throw new UnauthorizedAccessException("Workflow history requires staff access.");
                await RequireAssignedStaffAsync(issue.Id, user.Id);
            }

            var histories = await _queries.GetHistoryByIssueAsync(issue.Id);
            return histories.Select(history => new WorkflowHistoryDto
            {
                Id = history.Id,
                IssueId = history.IssueId,
                OldStatus = history.OldStatus,
                NewStatus = history.NewStatus,
                ChangedByName = history.ChangedBy?.FullName ?? "Unknown user",
                Note = history.Note,
                ChangedAt = history.ChangedAt
            }).ToList();
        }

        public async Task ReviewAsync(
            int issueId, int adminId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, adminId);

            var issue = await GetCivicIssueAsync(issueId);

            bool allowed =
                issue.Status == IssueStatusLookup.Reported ||
                issue.Status == IssueStatusLookup.NeedsInformation ||
                issue.Status == IssueStatusLookup.Reopened;

            if (!allowed)
            {
                throw new ValidationException(
                    "This issue cannot enter review.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            await SaveStatusAsync(
                issue,
                IssueStatusLookup.UnderReview,
                adminId,
                "Issue is under review.");

            await AddNotificationAsync(
                issue.ReporterId,
                issue.Id,
                "Issue under review",
                "Your report is being reviewed.");

            await transaction.CommitAsync();
        }

        public async Task AssignAsync(
            int issueId, int staffId, int adminId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, adminId);

            var staff = await FacadeValidation
                .GetActiveUserAsync(_users, staffId);

            if (staff.Role != UserRoleLookup.Staff)
            {
                throw new ValidationException(
                    "Selected user is not staff.");
            }

            var issue = await GetCivicIssueAsync(issueId);

            if (issue.Status != IssueStatusLookup.UnderReview)
            {
                throw new ValidationException(
                    "Issue must be under review before assignment.");
            }

            var existing = await _assignments
                .GetActiveByIssueAsync(issueId);

            if (existing != null)
            {
                throw new ValidationException(
                    "Issue already has an active assignment.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var assignment = new IssueAssignment
            {
                IssueId = issue.Id,
                StaffId = staff.Id,
                AssignedById = adminId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _assignments.AddAsync(assignment);

            await SaveStatusAsync(
                issue,
                IssueStatusLookup.Assigned,
                adminId,
                "Issue assigned to staff.");

            await AddNotificationAsync(
                staff.Id,
                issue.Id,
                "New issue assigned",
                "A civic issue has been assigned to you.");

            await AddNotificationAsync(
                issue.ReporterId,
                issue.Id,
                "Issue assigned",
                "Your civic issue has been assigned to staff.");

            await transaction.CommitAsync();
        }

        public async Task StartWorkAsync(
            int issueId, int staffId)
        {
            var staff = await FacadeValidation
                .GetActiveUserAsync(_users, staffId);

            if (staff.Role != UserRoleLookup.Staff)
            {
                throw new UnauthorizedAccessException(
                    "Staff access is required.");
            }

            var issue = await GetCivicIssueAsync(issueId);

            await RequireAssignedStaffAsync(
                issueId, staffId);

            if (issue.Status != IssueStatusLookup.Assigned)
            {
                throw new ValidationException(
                    "Only assigned issues can start work.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            await SaveStatusAsync(
                issue,
                IssueStatusLookup.InProgress,
                staffId,
                "Staff started work.");

            await AddNotificationAsync(
                issue.ReporterId,
                issue.Id,
                "Work started",
                "Work has started on your issue.");

            await transaction.CommitAsync();
        }

        public async Task ResolveAsync(
            int issueId, int staffId)
        {
            var staff = await FacadeValidation
                .GetActiveUserAsync(_users, staffId);

            if (staff.Role != UserRoleLookup.Staff)
            {
                throw new UnauthorizedAccessException(
                    "Staff access is required.");
            }

            var issue = await GetCivicIssueAsync(issueId);

            await RequireAssignedStaffAsync(
                issueId, staffId);

            if (issue.Status != IssueStatusLookup.InProgress)
            {
                throw new ValidationException(
                    "Only issues in progress can be resolved.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            issue.ResolvedAt = DateTime.UtcNow;

            await SaveStatusAsync(
                issue,
                IssueStatusLookup.Resolved,
                staffId,
                "Issue marked as resolved.");

            await AddNotificationAsync(
                issue.ReporterId,
                issue.Id,
                "Issue resolved",
                "Your reported issue has been resolved.");

            await transaction.CommitAsync();
        }

        public async Task RejectAsync(
            int issueId, int adminId, string reason)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, adminId);

            if (string.IsNullOrWhiteSpace(reason) ||
                reason.Trim().Length > 1000)
            {
                throw new ValidationException(
                    "A rejection reason of 1 to 1000 characters is required.");
            }

            var issue = await GetCivicIssueAsync(issueId);

            if (issue.Status != IssueStatusLookup.UnderReview)
            {
                throw new ValidationException(
                    "Only issues under review can be rejected.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            await SaveStatusAsync(
                issue,
                IssueStatusLookup.Rejected,
                adminId,
                reason.Trim());

            await AddNotificationAsync(
                issue.ReporterId,
                issue.Id,
                "Issue review completed",
                "Your issue was rejected. Reason: " + reason.Trim());

            await transaction.CommitAsync();
        }

        private async Task<Issue> GetCivicIssueAsync(
            int issueId)
        {
            if (issueId <= 0)
                throw new KeyNotFoundException("Valid issue ID is required.");

            var issue = await _issues.GetByIdAsync(issueId);

            if (issue == null ||
                issue.IsArchived ||
                issue.IssueType != IssueTypeLookup.PublicCivic)
            {
                throw new KeyNotFoundException(
                    "Active civic issue not found.");
            }

            return issue;
        }

        private async Task RequireAssignedStaffAsync(
            int issueId, int staffId)
        {
            var assignment = await _assignments
                .GetActiveByIssueAsync(issueId);

            if (assignment?.StaffId != staffId)
            {
                throw new UnauthorizedAccessException(
                    "This issue is not assigned to you.");
            }
        }

        private async Task SaveStatusAsync(
            Issue issue,
            byte newStatus,
            int actorId,
            string note)
        {
            var previousStatus = issue.Status;

            if (previousStatus == newStatus)
            {
                throw new ValidationException(
                    "Issue is already in this status.");
            }

            if (string.IsNullOrWhiteSpace(note) || note.Length > 1000)
                throw new ValidationException("A valid workflow note is required.");

            issue.Status = newStatus;
            issue.UpdatedAt = DateTime.UtcNow;

            await _issues.UpdateAsync(issue);

            var history = new IssueStatusHistory
            {
                IssueId = issue.Id,
                OldStatus = previousStatus,
                NewStatus = newStatus,
                ChangedById = actorId,
                Note = note,
                ChangedAt = DateTime.UtcNow
            };

            await _history.AddAsync(history);
        }

        private async Task AddNotificationAsync(
            int userId,
            int issueId,
            string title,
            string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                IssueId = issueId,
                Title = title,
                Message = message,
                NotificationType = "IssueStatus",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notifications.AddAsync(notification);
        }
    }
}
