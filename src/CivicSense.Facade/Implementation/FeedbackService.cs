
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
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

        public async Task<FeedbackDto> SubmitAsync(
            int issueId,
            int actorId,
            FeedbackCreateDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            FacadeValidation.Validate(dto);

            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            var issue = await _issues.GetByIdAsync(issueId);

            if (issue == null || issue.IsArchived)
            {
                throw new KeyNotFoundException(
                    "Issue not found.");
            }

            if (issue.ReporterId != user.Id)
            {
                throw new UnauthorizedAccessException(
                    "Only the reporter can submit feedback.");
            }

            if (issue.Status != IssueStatus.Resolved)
            {
                throw new ValidationException(
                    "Feedback is allowed after resolution.");
            }

            if (dto.Rating < 1 || dto.Rating > 5)
            {
                throw new ValidationException(
                    "Rating must be between 1 and 5.");
            }

            var existing = await _feedbacks
                .GetByIssueAndUserAsync(issueId, actorId);

            if (existing != null)
            {
                throw new ValidationException(
                    "Feedback already exists for this issue.");
            }

            var feedback = new Feedback
            {
                IssueId = issueId,
                UserId = user.Id,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim(),
                IsResolutionConfirmed = dto.IsResolutionConfirmed,
                CreatedAt = DateTime.UtcNow
            };

            await _feedbacks.AddAsync(feedback);

            return Map(feedback);
        }

        private static FeedbackDto Map(Feedback feedback)
        {
            return new FeedbackDto
            {
                Id = feedback.Id,
                IssueId = feedback.IssueId,
                Rating = feedback.Rating,
                Comment = feedback.Comment,
                IsResolutionConfirmed =
                    feedback.IsResolutionConfirmed,
                CreatedAt = feedback.CreatedAt
            };
        }
    }
}
