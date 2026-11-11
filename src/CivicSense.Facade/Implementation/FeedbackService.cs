
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.Interface;

namespace CivicSense.Facade.Implementation
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbacks;
        private readonly IIssueRepository _issues;
        private readonly IUserRepository _users;

        public FeedbackService(
            IFeedbackRepository feedbacks,
            IIssueRepository issues,
            IUserRepository users)
        {
            _feedbacks = feedbacks;
            _issues = issues;
            _users = users;
        }

        public async Task<Feedback> SubmitAsync(
            int issueId,
            int actorId,
            int rating,
            bool resolutionConfirmed,
            string? comment)
        {
            await FacadeValidation.GetActiveUserAsync(
                _users, actorId);

            var issue = await _issues.GetByIdAsync(issueId);

            if (issue == null || issue.ReporterId != actorId)
            {
                throw new UnauthorizedAccessException(
                    "Only the reporter can submit feedback.");
            }

            if (issue.Status != IssueStatus.Resolved)
            {
                throw new ValidationException(
                    "Feedback is allowed only after resolution.");
            }

            if (rating < 1 || rating > 5)
            {
                throw new ValidationException(
                    "Rating must be between 1 and 5.");
            }

            if (comment?.Trim().Length > 2000)
            {
                throw new ValidationException(
                    "Feedback comment cannot exceed 2000 characters.");
            }

            // Check feedback for this issue and user only.
            var existing = await _feedbacks
                .GetByIssueAndUserAsync(issueId, actorId);

            if (existing != null)
            {
                throw new ValidationException(
                    "Feedback has already been submitted.");
            }

            var feedback = new Feedback
            {
                IssueId = issueId,
                UserId = actorId,
                Rating = rating,
                Comment = comment?.Trim(),
                IsResolutionConfirmed = resolutionConfirmed,
                CreatedAt = DateTime.UtcNow
            };

            await _feedbacks.AddAsync(feedback);

            return feedback;
        }
    }
}
