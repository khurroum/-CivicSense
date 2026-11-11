
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

        public IssueConfirmationService(
            IIssueConfirmationRepository confirmations,
            IIssueRepository issues,
            IUserRepository users)
        {
            _confirmations = confirmations;
            _issues = issues;
            _users = users;
        }

        public async Task ConfirmAsync(
            int issueId, int actorId, bool confirmed)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            if (user.Role != UserRole.Resident)
            {
                throw new UnauthorizedAccessException(
                    "Only residents can confirm issues.");
            }

            var issue = await IssueAccess.RequireVisibleAsync(
                _issues, _users, issueId, actorId);

            if (issue.IssueType != IssueType.PublicCivic)
            {
                throw new InvalidOperationException(
                    "Private issues cannot be confirmed publicly.");
            }

            // Search for this specific user's confirmation.
            var existing = await _confirmations
                .GetByIssueAndUserAsync(issueId, actorId);

            if (existing != null)
            {
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
