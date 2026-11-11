
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.Interface;
using Microsoft.EntityFrameworkCore;

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

        public IssueWorkflowService(
            ApplicationDbContext context,
            IIssueRepository issues,
            IUserRepository users,
            IIssueAssignmentRepository assignments,
            IIssueStatusHistoryRepository history,
            INotificationRepository notifications)
        {
            _context = context;
            _issues = issues;
            _users = users;
            _assignments = assignments;
            _history = history;
            _notifications = notifications;
        }

        public async Task ReviewAsync(int issueId, int adminId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, adminId);

            var issue = await GetCivicIssueAsync(issueId);

            if (issue.Status != IssueStatus.Reported &&
                issue.Status != IssueStatus.NeedsInformation &&
                issue.Status != IssueStatus.Reopened)
            {
                throw new ValidationException(
                    "This issue cannot enter review.");
            }

            await ChangeStatusAsync(
                issue, IssueStatus.UnderReview,
                adminId, "Issue is under review.");
        }

        public async Task AssignAsync(
            int issueId, int staffId, int adminId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, adminId);

            var staff = await FacadeValidation
                .GetActiveUserAsync(_users, staffId);

            if (staff.Role != UserRole.Staff)
                throw new ValidationException(
                    "Selected user is not a staff member.");

            var issue = await GetCivicIssueAsync(issueId);

            if (issue.Status != IssueStatus.UnderReview)
                throw new ValidationException(
                    "Issue must be under review before assignment.");

            var existing = await _assignments
                .GetActiveByIssueAsync(issueId);

            if (existing != null)
                throw new ValidationException(
                    "Issue already has an active assignment.");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var assignment = new IssueAssignment
            {
                IssueId = issueId,
                StaffId = staffId,
                AssignedById = adminId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _assignments.AddAsync(assignment);

            await SaveStatusAsync(
                issue, IssueStatus.Assigned,
                adminId, "Issue assigned to staff.");

            await AddNotificationAsync(
                staffId, issueId,
                "New issue assigned",
                "A civic issue has been assigned to you.");

            await transaction.CommitAsync();
        }

        public async Task StartWorkAsync(
            int issueId, int staffId)
        {
            var staff = await FacadeValidation
                .GetActiveUserAsync(_users, staffId);

            if (staff.Role != UserRole.Staff)
                throw new UnauthorizedAccessException();

            var issue = await GetCivicIssueAsync(issueId);
            await RequireAssignedStaffAsync(issueId, staffId);

            if (issue.Status != IssueStatus.Assigned)
                throw new ValidationException(
                    "Issue is not ready to start.");

            await ChangeStatusAsync(
                issue, IssueStatus.InProgress,
                staffId, "Work started.");
        }

        public async Task ResolveAsync(
            int issueId, int staffId)
        {
            var staff = await FacadeValidation
                .GetActiveUserAsync(_users, staffId);

            if (staff.Role != UserRole.Staff)
                throw new UnauthorizedAccessException();

            var issue = await GetCivicIssueAsync(issueId);
            await RequireAssignedStaffAsync(issueId, staffId);

            if (issue.Status != IssueStatus.InProgress)
                throw new ValidationException(
                    "Issue is not in progress.");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            issue.ResolvedAt = DateTime.UtcNow;

            await SaveStatusAsync(
                issue, IssueStatus.Resolved,
                staffId, "Issue marked as resolved.");

            await AddNotificationAsync(
                issue.ReporterId, issue.Id,
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
                throw new ValidationException(
                    "A valid rejection reason is required.");

            var issue = await GetCivicIssueAsync(issueId);

            if (issue.Status != IssueStatus.UnderReview)
                throw new ValidationException(
                    "Only issues under review can be rejected.");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            await SaveStatusAsync(
                issue, IssueStatus.Rejected,
                adminId, reason.Trim());

            await AddNotificationAsync(
                issue.ReporterId, issue.Id,
                "Issue review completed",
                "Your issue was rejected. Please review its status.");

            await transaction.CommitAsync();
        }

        private async Task<Issue> GetCivicIssueAsync(int id)
        {
            var issue = await _issues.GetByIdAsync(id);

            if (issue == null ||
                issue.IsArchived ||
                issue.IssueType != IssueType.PublicCivic)
                throw new ValidationException(
                    "Active public civic issue not found.");

            return issue;
        }

        private async Task RequireAssignedStaffAsync(
            int issueId, int staffId)
        {
            var assignment = await _assignments
                .GetActiveByIssueAsync(issueId);

            if (assignment?.StaffId != staffId)
                throw new UnauthorizedAccessException(
                    "Issue is not assigned to this staff member.");
        }

        private async Task ChangeStatusAsync(
            Issue issue, IssueStatus status,
            int actorId, string note)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            await SaveStatusAsync(issue, status, actorId, note);

            await transaction.CommitAsync();
        }

        private async Task SaveStatusAsync(
            Issue issue, IssueStatus status,
            int actorId, string note)
        {
            var oldStatus = issue.Status;

            issue.Status = status;
            issue.UpdatedAt = DateTime.UtcNow;

            await _issues.UpdateAsync(issue);

            await _history.AddAsync(new IssueStatusHistory
            {
                IssueId = issue.Id,
                OldStatus = oldStatus,
                NewStatus = status,
                ChangedById = actorId,
                Note = note,
                ChangedAt = DateTime.UtcNow
            });
        }

        private async Task AddNotificationAsync(
            int userId, int issueId,
            string title, string message)
        {
            await _notifications.AddAsync(new Notification
            {
                UserId = userId,
                IssueId = issueId,
                Title = title,
                Message = message,
                NotificationType = "IssueStatus",
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
