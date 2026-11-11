
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;

namespace CivicSense.Facade.Implementation
{
    internal static class IssueAccess
    {
        public static bool IsPublished(Issue issue)
        {
            return issue.IssueType == IssueType.PublicCivic &&
                !issue.IsArchived &&
                (issue.Status == IssueStatus.Assigned ||
                 issue.Status == IssueStatus.InProgress ||
                 issue.Status == IssueStatus.Resolved);
        }

        public static async Task<Issue> RequireVisibleAsync(
            IIssueRepository issues,
            IUserRepository users,
            IIssueAssignmentRepository assignments,
            int issueId,
            int actorId)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(users, actorId);

            var issue = await issues.GetByIdAsync(issueId);

            if (issue == null || issue.IsArchived)
            {
                throw new KeyNotFoundException(
                    "Issue not found.");
            }

            // Owners may view their own reports.
            if (issue.ReporterId == user.Id)
                return issue;

            // A private report belongs only to its reporter.
            if (issue.IssueType == IssueType.PersonalPrivate)
            {
                throw new UnauthorizedAccessException(
                    "You cannot access this private issue.");
            }

            // Admins may review civic issues.
            if (user.Role == UserRole.Admin)
                return issue;

            // Assigned staff may access their civic issue.
            if (user.Role == UserRole.Staff)
            {
                var assignment = await assignments
                    .GetActiveByIssueAsync(issueId);

                if (assignment?.StaffId == user.Id)
                    return issue;
            }

            // Published civic issues can be viewed by
            // other authenticated community members.
            if (IsPublished(issue))
                return issue;

            throw new UnauthorizedAccessException(
                "This issue is not publicly available.");
        }
    }
}
