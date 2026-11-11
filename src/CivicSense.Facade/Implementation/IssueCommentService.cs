
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.Interface;

namespace CivicSense.Facade.Implementation
{
    public class IssueCommentService : IIssueCommentService
    {
        private readonly IIssueCommentRepository _comments;
        private readonly IIssueRepository _issues;
        private readonly IUserRepository _users;
        private readonly IIssueAssignmentRepository _assignments;

        public IssueCommentService(
            IIssueCommentRepository comments,
            IIssueRepository issues,
            IUserRepository users,
            IIssueAssignmentRepository assignments)
        {
            _comments = comments;
            _issues = issues;
            _users = users;
            _assignments = assignments;
        }

        public async Task<IssueComment> AddAsync(
            int issueId, int actorId,
            string content, bool isInternal)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            var issue = await IssueAccess.RequireVisibleAsync(
                _issues, _users, issueId, actorId);

            if (string.IsNullOrWhiteSpace(content) ||
                content.Trim().Length > 2000)
            {
                throw new ValidationException(
                    "Comment must contain 1 to 2000 characters.");
            }

            if (isInternal)
            {
                if (issue.IssueType != IssueType.PublicCivic)
                {
                    throw new UnauthorizedAccessException(
                        "Internal notes are only allowed for civic issues.");
                }

                var assignment = await _assignments
                    .GetActiveByIssueAsync(issueId);

                bool authorized =
                    user.Role == UserRole.Admin ||
                    (user.Role == UserRole.Staff &&
                     assignment?.StaffId == actorId);

                if (!authorized)
                {
                    throw new UnauthorizedAccessException(
                        "Internal notes require authorized staff access.");
                }
            }

            var comment = new IssueComment
            {
                IssueId = issueId,
                UserId = actorId,
                Content = content.Trim(),
                IsInternal = isInternal,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _comments.AddAsync(comment);

            return comment;
        }

        public async Task<List<IssueComment>> GetByIssueAsync(
            int issueId, int actorId)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            var issue = await IssueAccess.RequireVisibleAsync(
                _issues, _users, issueId, actorId);

            bool canReadInternal = false;

            if (issue.IssueType == IssueType.PublicCivic)
            {
                var assignment = await _assignments
                    .GetActiveByIssueAsync(issueId);

                canReadInternal =
                    user.Role == UserRole.Admin ||
                    (user.Role == UserRole.Staff &&
                     assignment?.StaffId == actorId);
            }

            // Query only comments associated with this issue.
            var comments = await _comments.GetByIssueAsync(issueId);

            return comments
                .Where(x => !x.IsInternal || canReadInternal)
                .OrderBy(x => x.CreatedAt)
                .ToList();
        }
    }
}
