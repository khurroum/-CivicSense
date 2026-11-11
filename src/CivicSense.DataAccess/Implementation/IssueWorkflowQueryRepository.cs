
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicSense.DataAccess.Implementation
{
    public class IssueWorkflowQueryRepository
        : IIssueWorkflowQueryRepository
    {
        private readonly ApplicationDbContext _context;

        public IssueWorkflowQueryRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ADMIN REVIEW QUEUE
        public async Task<List<Issue>> GetAdminQueueAsync()
        {
            return await _context.Issues
                .AsNoTracking()
                .Where(x =>
                    !x.IsArchived &&
                    x.IssueType == IssueType.PublicCivic &&
                    (
                        x.Status == IssueStatus.Reported ||
                        x.Status == IssueStatus.UnderReview ||
                        x.Status == IssueStatus.NeedsInformation ||
                        x.Status == IssueStatus.Reopened
                    ))
                .Include(x => x.Reporter)
                .Include(x => x.Category)
                .Include(x => x.Assignments
                    .Where(a => a.IsActive))
                    .ThenInclude(a => a.Staff)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        // STAFF ASSIGNED ISSUES
        public async Task<List<IssueAssignment>>
            GetActiveAssignmentsForStaffAsync(int staffId)
        {
            return await _context.IssueAssignments
                .AsNoTracking()
                .Where(x =>
                    x.StaffId == staffId &&
                    x.IsActive &&
                    !x.Issue.IsArchived &&
                    x.Issue.IssueType == IssueType.PublicCivic)
                .Include(x => x.Issue)
                    .ThenInclude(x => x.Category)
                .Include(x => x.Issue)
                    .ThenInclude(x => x.Reporter)
                .OrderByDescending(x => x.AssignedAt)
                .ToListAsync();
        }

        // ACTIVE STAFF ACCOUNTS
        public async Task<List<User>> GetActiveStaffAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Role == UserRole.Staff)
                .OrderBy(x => x.FullName)
                .ToListAsync();
        }

        // ISSUE STATUS HISTORY
        public async Task<List<IssueStatusHistory>>
            GetHistoryByIssueAsync(int issueId)
        {
            return await _context.IssueStatusHistories
                .AsNoTracking()
                .Where(x => x.IssueId == issueId)
                .Include(x => x.ChangedBy)
                .OrderBy(x => x.ChangedAt)
                .ToListAsync();
        }
    }
}
