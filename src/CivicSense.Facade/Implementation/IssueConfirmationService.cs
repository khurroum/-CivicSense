
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.Interface;

namespace CivicSense.Facade.Implementation
{
    public class IssueConfirmationService
        : IIssueConfirmationService
    {
        private readonly IIssueConfirmationRepository _confirmations;
        private readonly IIssueRepository _issues;
        private readonly IUserRepository _users;
        private readonly IIssueAssignmentRepository _assignments;

        public IssueConfirmationService(
            IIssueConfirmationRepository confirmations,
            IIssueRepository issues,
            IUserRepository users,
            IIssueAssignmentRepository assignments)
        {
            _confirmations = confirmations;
            _issues = issues;
            _users = users;
            _assignments = assignments;
        }

        public async Task ConfirmAsync(
            int issueId, int actorId, bool confirmed)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            if (user.Role != UserRole.Resident)
            {
                throw new UnauthorizedAccessException(
                    "Only residents can confirm civic issues.");
            }

            var issue = await IssueAccess.RequireVisibleAsync(
                _issues, _users, _assignments,
                issueId, actorId);

            if (!IssueAccess.IsPublished(issue))
            {
                throw new ValidationException(
                    "Only published civic issues can be confirmed.");
            }

            var existing = await _confirmations
                .GetByIssueAndUserAsync(issueId, actorId);

            if (existing != null)
            {
                if (existing.IsConfirmed == confirmed)
                    return;

                existing.IsConfirmed = confirmed;

                await _confirmations.UpdateAsync(existing);
                return;
            }

            var confirmation = new IssueConfirmation
            {
                IssueId = issueId,
                UserId = actorId,
                IsConfirmed = confirmed,
                CreatedAt = DateTime.UtcNow
            };

            await _confirmations.AddAsync(confirmation);
        }
    }
}
