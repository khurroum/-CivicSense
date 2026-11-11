
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
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

        public async Task<CommentDto> AddAsync(
            int issueId,
            int actorId,
            CommentCreateDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            FacadeValidation.Validate(dto);

            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            var issue = await IssueAccess.RequireVisibleAsync(
                _issues, _users, _assignments,
                issueId, actorId);

            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                throw new ValidationException(
                    "Comment cannot be empty.");
            }

            string content = dto.Content.Trim();

            if (content.Length > 2000)
            {
                throw new ValidationException(
                    "Comment cannot exceed 2000 characters.");
            }

            if (dto.IsInternal)
            {
                if (issue.IssueType != IssueType.PublicCivic)
                {
                    throw new UnauthorizedAccessException(
                        "Internal notes are for civic issues only.");
                }

                var assignment = await _assignments
                    .GetActiveByIssueAsync(issueId);

                bool allowed =
                    user.Role == UserRole.Admin ||
                    (user.Role == UserRole.Staff &&
                     assignment?.StaffId == user.Id);

                if (!allowed)
                {
                    throw new UnauthorizedAccessException(
                        "You cannot create internal notes.");
                }
            }
            else if (issue.IssueType == IssueType.PublicCivic &&
                     user.Role == UserRole.Staff &&
                     !IssueAccess.IsPublished(issue))
            {
                // Assigned staff may add internal notes,
                // but public comments require publication.
                throw new ValidationException(
                    "This issue is not open for public comments.");
            }

            var comment = new IssueComment
            {
                IssueId = issue.Id,
                UserId = user.Id,
                Content = content,
                IsInternal = dto.IsInternal,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _comments.AddAsync(comment);

            return Map(comment);
        }

        public async Task<List<CommentDto>> GetByIssueAsync(
            int issueId, int actorId)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            var issue = await IssueAccess.RequireVisibleAsync(
                _issues, _users, _assignments,
                issueId, actorId);

            bool canReadInternal = false;

            if (issue.IssueType == IssueType.PublicCivic)
            {
                var assignment = await _assignments
                    .GetActiveByIssueAsync(issueId);

                canReadInternal =
                    user.Role == UserRole.Admin ||
                    (user.Role == UserRole.Staff &&
                     assignment?.StaffId == user.Id);
            }

            var comments = await _comments
                .GetByIssueAsync(issueId);

            return comments
                .Where(x => !x.IsInternal || canReadInternal)
                .Select(Map)
                .ToList();
        }

        private static CommentDto Map(IssueComment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                IssueId = comment.IssueId,
                AuthorId = comment.UserId,
                Content = comment.Content,
                IsInternal = comment.IsInternal,
                CreatedAt = comment.CreatedAt
            };
        }
    }
}
