
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;

namespace CivicSense.Facade.Implementation
{
    internal static class IssueAccess
    {
        public static async Task<Issue> RequireVisibleAsync(
            IIssueRepository issues,
            IUserRepository users,
            int issueId,
            int actorId)
        {
            await FacadeValidation.GetActiveUserAsync(
                users, actorId);

            var issue = await issues.GetByIdAsync(issueId);

            if (issue == null || issue.IsArchived)
                throw new KeyNotFoundException(
                    "Issue not found.");

            // Public civic issues can be viewed by
            // authenticated community members.
            if (issue.IssueType == IssueType.PublicCivic)
                return issue;

            // Private issues belong to their reporter.
            if (issue.ReporterId != actorId)
                throw new UnauthorizedAccessException(
                    "Access to this private issue is denied.");

            return issue;
        }
    }
}
