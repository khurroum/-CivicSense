
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicSense.DataAccess.Implementation
{
    public class IssueWorkflowQueryRepository : IIssueWorkflowQueryRepository
    {
        private readonly ApplicationDbContext _context;

        public IssueWorkflowQueryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<Issue>> GetAdminQueueAsync()
        {
            return _context.Issues.AsNoTracking()
                .Where(x => !x.IsArchived && x.IssueType == IssueTypeLookup.PublicCivic &&
                    (x.Status == IssueStatusLookup.Reported ||
                     x.Status == IssueStatusLookup.UnderReview ||
                     x.Status == IssueStatusLookup.NeedsInformation ||
                     x.Status == IssueStatusLookup.Reopened))
                .Include(x => x.Reporter)
                .Include(x => x.Category)
                .Include(x => x.Assignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.Staff)
                .OrderBy(x => x.CreatedAt).ToListAsync();
        }

        public Task<List<IssueAssignment>> GetActiveAssignmentsForStaffAsync(int staffId)
        {
            return _context.IssueAssignments.AsNoTracking()
                .Where(x => x.StaffId == staffId && x.IsActive &&
                    !x.Issue.IsArchived && x.Issue.IssueType == IssueTypeLookup.PublicCivic)
                .Include(x => x.Issue).ThenInclude(x => x.Category)
                .Include(x => x.Issue).ThenInclude(x => x.Reporter)
                .OrderByDescending(x => x.AssignedAt).ToListAsync();
        }

        public Task<List<User>> GetActiveStaffAsync()
        {
            return _context.Users.AsNoTracking()
                .Where(x => x.IsActive && x.Role == UserRoleLookup.Staff)
                .OrderBy(x => x.FullName).ToListAsync();
        }

        public Task<List<IssueStatusHistory>> GetHistoryByIssueAsync(int issueId)
        {
            return _context.IssueStatusHistories.AsNoTracking()
                .Where(x => x.IssueId == issueId)
                .Include(x => x.ChangedBy)
                .OrderBy(x => x.ChangedAt).ThenBy(x => x.Id).ToListAsync();
        }
    }
}
