
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;

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

        // ==========================================
        // 1. ADMIN REVIEW QUEUE
        // ==========================================

        public async Task<List<WorkflowQueueItemDto>>
            GetAdminQueueAsync(int adminId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, adminId);

            var issues = await _queries.GetAdminQueueAsync();

            return issues
                .Select(MapQueueItem)
                .ToList();
        }

        // ==========================================
        // 2. ACTIVE STAFF DROPDOWN
        // ==========================================

        public async Task<List<WorkflowStaffOptionDto>>
            GetActiveStaffAsync(int adminId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, adminId);

            var staffMembers =
                await _queries.GetActiveStaffAsync();

            return staffMembers
                .Select(user => new WorkflowStaffOptionDto
                {
                    Id = user.Id,
                    FullName = user.FullName
                })
                .ToList();
        }

        // ==========================================
        // 3. STAFF ASSIGNED ISSUES
        // ==========================================

        public async Task<List<WorkflowAssignmentDto>>
            GetMyAssignmentsAsync(int staffId)
        {
            var staff = await FacadeValidation
                .GetActiveUserAsync(_users, staffId);

            if (staff.Role != UserRole.Staff)
            {
                throw new UnauthorizedAccessException(
                    "Staff access is required.");
            }

            var assignments = await _queries
                .GetActiveAssignmentsForStaffAsync(staffId);

            return assignments
                .Select(MapAssignment)
                .ToList();
        }

        // ==========================================
        // 4. RESTRICTED ISSUE STATUS HISTORY
        // ==========================================

        public async Task<List<WorkflowHistoryDto>>
            GetHistoryAsync(int issueId, int actorId)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            var issue = await GetCivicIssueAsync(issueId);

            // Full workflow history can contain
            // administrative or internal notes.
            // Restrict it to Admin or assigned Staff.
            if (user.Role != UserRole.Admin)
            {
                if (user.Role != UserRole.Staff)
                {
                    throw new UnauthorizedAccessException(
                        "Workflow history requires staff access.");
                }

                await RequireAssignedStaffAsync(
                    issue.Id, user.Id);
            }

            var histories = await _queries
                .GetHistoryByIssueAsync(issue.Id);

            return histories
                .Select(MapHistory)
                .ToList();
        }

        // ==========================================
        // 5. ADMIN: REVIEW ISSUE
        // ==========================================

        public async Task ReviewAsync(
            int issueId, int adminId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, adminId);

            var issue = await GetCivicIssueAsync(issueId);

            bool allowed =
                issue.Status == IssueStatus.Reported ||
                issue.Status == IssueStatus.NeedsInformation ||
                issue.Status == IssueStatus.Reopened;

            if (!allowed)
            {
                throw new ValidationException(
                    "This issue cannot enter review.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            await SaveStatusAsync(
                issue,
                IssueStatus.UnderReview,
                adminId,
                "Issue is under review.");

            await AddNotificationAsync(
                issue.ReporterId,
                issue.Id,
                "Issue under review",
                "Your civic report is being reviewed.");

            await transaction.CommitAsync();
        }

        // ==========================================
        // 6. ADMIN: ASSIGN ISSUE TO STAFF
        // ==========================================

        public async Task AssignAsync(
            int issueId, int staffId, int adminId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, adminId);

            var staff = await FacadeValidation
                .GetActiveUserAsync(_users, staffId);

            if (staff.Role != UserRole.Staff)
            {
                throw new ValidationException(
                    "Selected user is not an active Staff member.");
            }

            var issue = await GetCivicIssueAsync(issueId);

            if (issue.Status != IssueStatus.UnderReview)
            {
                throw new ValidationException(
                    "Issue must be under review before assignment.");
            }

            var existing = await _assignments
                .GetActiveByIssueAsync(issueId);

            if (existing != null)
            {
                throw new ValidationException(
                    "This issue already has an active assignment.");
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
                IssueStatus.Assigned,
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
                "Your issue has been assigned to staff.");

            await transaction.CommitAsync();
        }

        // ==========================================
        // 7. STAFF: START WORK
        // ==========================================

        public async Task StartWorkAsync(
            int issueId, int staffId)
        {
            var staff = await FacadeValidation
                .GetActiveUserAsync(_users, staffId);

            if (staff.Role != UserRole.Staff)
            {
                throw new UnauthorizedAccessException(
                    "Staff access is required.");
            }

            var issue = await GetCivicIssueAsync(issueId);

            await RequireAssignedStaffAsync(
                issueId, staff.Id);

            if (issue.Status != IssueStatus.Assigned)
            {
                throw new ValidationException(
                    "Only assigned issues can start work.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            await SaveStatusAsync(
                issue,
                IssueStatus.InProgress,
                staff.Id,
                "Staff started work on the issue.");

            await AddNotificationAsync(
                issue.ReporterId,
                issue.Id,
                "Work started",
                "Work has started on your reported issue.");

            await transaction.CommitAsync();
        }

        // ==========================================
        // 8. STAFF: RESOLVE ISSUE
        // ==========================================

        public async Task ResolveAsync(
            int issueId, int staffId)
        {
            var staff = await FacadeValidation
                .GetActiveUserAsync(_users, staffId);

            if (staff.Role != UserRole.Staff)
            {
                throw new UnauthorizedAccessException(
                    "Staff access is required.");
            }

            var issue = await GetCivicIssueAsync(issueId);

            await RequireAssignedStaffAsync(
                issueId, staff.Id);

            if (issue.Status != IssueStatus.InProgress)
            {
                throw new ValidationException(
                    "Only in-progress issues can be resolved.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            issue.ResolvedAt = DateTime.UtcNow;

            await SaveStatusAsync(
                issue,
                IssueStatus.Resolved,
                staff.Id,
                "Issue marked as resolved.");

            await AddNotificationAsync(
                issue.ReporterId,
                issue.Id,
                "Issue resolved",
                "Your reported issue has been resolved.");

            await transaction.CommitAsync();
        }

        // ==========================================
        // 9. ADMIN: REJECT ISSUE
        // ==========================================

        public async Task RejectAsync(
            int issueId, int adminId, string reason)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, adminId);

            if (string.IsNullOrWhiteSpace(reason) ||
                reason.Trim().Length > 1000)
            {
                throw new ValidationException(
                    "A rejection reason of 1 to 1000 " +
                    "characters is required.");
            }

            var issue = await GetCivicIssueAsync(issueId);

            if (issue.Status != IssueStatus.UnderReview)
            {
                throw new ValidationException(
                    "Only issues under review can be rejected.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            await SaveStatusAsync(
                issue,
                IssueStatus.Rejected,
                adminId,
                reason.Trim());

            await AddNotificationAsync(
                issue.ReporterId,
                issue.Id,
                "Issue review completed",
                "Your issue was rejected. " +
                "Please review the reason.");

            await transaction.CommitAsync();
        }

        // ==========================================
        // PRIVATE: VALIDATE CIVIC ISSUE
        // ==========================================

        private async Task<Issue> GetCivicIssueAsync(
            int issueId)
        {
            if (issueId <= 0)
            {
                throw new KeyNotFoundException(
                    "Valid issue ID is required.");
            }

            var issue = await _issues.GetByIdAsync(issueId);

            if (issue == null ||
                issue.IsArchived ||
                issue.IssueType != IssueType.PublicCivic)
            {
                throw new KeyNotFoundException(
                    "Active civic issue not found.");
            }

            return issue;
        }

        // ==========================================
        // PRIVATE: VERIFY STAFF ASSIGNMENT
        // ==========================================

        private async Task RequireAssignedStaffAsync(
            int issueId, int staffId)
        {
            var assignment = await _assignments
                .GetActiveByIssueAsync(issueId);

            if (assignment == null ||
                assignment.StaffId != staffId)
            {
                throw new UnauthorizedAccessException(
                    "This issue is not assigned to you.");
            }
        }

        // ==========================================
        // PRIVATE: SAVE ISSUE STATUS + HISTORY
        // Must be called inside a database transaction.
        // ==========================================

        private async Task SaveStatusAsync(
            Issue issue,
            IssueStatus newStatus,
            int actorId,
            string note)
        {
            IssueStatus previousStatus = issue.Status;

            if (previousStatus == newStatus)
            {
                throw new ValidationException(
                    "Issue is already in this status.");
            }

            if (string.IsNullOrWhiteSpace(note) ||
                note.Length > 1000)
            {
                throw new ValidationException(
                    "A valid workflow note is required.");
            }

            issue.Status = newStatus;
            issue.UpdatedAt = DateTime.UtcNow;

            // Existing repository saves through
            // the same scoped ApplicationDbContext.
            //
            // Issue.RowVersion is an EF Core
            // concurrency token. A conflicting
            // database update will throw.
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

        // ==========================================
        // PRIVATE: SAVE DATABASE NOTIFICATION
        // Must be called inside a database transaction.
        // ==========================================

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

        // ==========================================
        // PRIVATE: MAP ADMIN QUEUE ITEM
        // ==========================================

        private static WorkflowQueueItemDto MapQueueItem(
            Issue issue)
        {
            return new WorkflowQueueItemDto
            {
                IssueId = issue.Id,
                Title = issue.Title,
                CategoryName =
                    issue.Category?.Name ?? "Unknown category",
                ReporterName =
                    issue.Reporter?.FullName ?? "Unknown reporter",
                Status = issue.Status,
                Priority = issue.Priority,
                ReportedAt = issue.CreatedAt
            };
        }

        // ==========================================
        // PRIVATE: MAP STAFF ASSIGNMENT
        // ==========================================

        private static WorkflowAssignmentDto MapAssignment(
            IssueAssignment assignment)
        {
            var issue = assignment.Issue;

            return new WorkflowAssignmentDto
            {
                AssignmentId = assignment.Id,
                IssueId = issue.Id,
                Title = issue.Title,
                Description = issue.Description,
                Address = issue.Address,
                CategoryName =
                    issue.Category?.Name ?? "Unknown category",
                Status = issue.Status,
                Priority = issue.Priority,
                AssignedAt = assignment.AssignedAt,
                ExpectedCompletionAt =
                    assignment.ExpectedCompletionAt,
                AssignmentNote = assignment.AssignmentNote
            };
        }

        // ==========================================
        // PRIVATE: MAP STATUS HISTORY
        // ==========================================

        private static WorkflowHistoryDto MapHistory(
            IssueStatusHistory history)
        {
            return new WorkflowHistoryDto
            {
                Id = history.Id,
                IssueId = history.IssueId,
                OldStatus = history.OldStatus,
                NewStatus = history.NewStatus,
                ChangedByName =
                    history.ChangedBy?.FullName ?? "Unknown user",
                Note = history.Note,
                ChangedAt = history.ChangedAt
            };
        }
    }
}
