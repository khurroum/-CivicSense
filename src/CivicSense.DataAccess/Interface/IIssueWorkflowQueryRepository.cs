
using CivicSense.Entities;

namespace CivicSense.DataAccess.Interface
{
    public interface IIssueWorkflowQueryRepository
    {
        // Admin: issues waiting for review, assignment or rejection.
        Task<List<Issue>> GetAdminQueueAsync();

        // Staff: issues currently assigned to this staff member.
        Task<List<IssueAssignment>>
            GetActiveAssignmentsForStaffAsync(int staffId);

        // Admin: retrieve active Staff accounts for assignment.
        Task<List<User>> GetActiveStaffAsync();

        // Retrieve chronological status history for an issue.
        Task<List<IssueStatusHistory>>
            GetHistoryByIssueAsync(int issueId);
    }
}
