
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.Interface;

namespace CivicSense.Facade.Implementation
{
    public class AIAnalysisService : IAIAnalysisService
    {
        private readonly IAIAnalysisRepository _analyses;
        private readonly IIssueRepository _issues;
        private readonly IUserRepository _users;

        public AIAnalysisService(
            IAIAnalysisRepository analyses,
            IIssueRepository issues,
            IUserRepository users)
        {
            _analyses = analyses;
            _issues = issues;
            _users = users;
        }

        // Trusted server-side AI integration only.
        // Never expose this as unrestricted public CRUD.
        public async Task<AIAnalysis> RecordSuggestionAsync(
            AIAnalysis suggestion)
        {
            if (suggestion == null)
                throw new ArgumentNullException(
                    nameof(suggestion));

            var issue = await _issues.GetByIdAsync(
                suggestion.IssueId);

            if (issue == null || issue.IsArchived)
                throw new ValidationException(
                    "A valid issue is required.");

            if (string.IsNullOrWhiteSpace(suggestion.Provider) ||
                string.IsNullOrWhiteSpace(suggestion.ModelName))
                throw new ValidationException(
                    "AI provider and model name are required.");

            if (suggestion.ConfidenceScore.HasValue &&
                (suggestion.ConfidenceScore < 0 ||
                 suggestion.ConfidenceScore > 1))
                throw new ValidationException(
                    "Confidence must be between 0 and 1.");

            if (suggestion.SuggestedIssueType.HasValue &&
                !Enum.IsDefined(
                    suggestion.SuggestedIssueType.Value))
                throw new ValidationException(
                    "Invalid suggested issue type.");

            if (suggestion.SuggestedPriority.HasValue &&
                !Enum.IsDefined(
                    suggestion.SuggestedPriority.Value))
                throw new ValidationException(
                    "Invalid suggested priority.");

            suggestion.Id = 0;
            suggestion.IsReviewed = false;
            suggestion.ReviewedById = null;
            suggestion.ReviewedAt = null;
            suggestion.CreatedAt = DateTime.UtcNow;

            await _analyses.AddAsync(suggestion);

            return suggestion;
        }

        public async Task ReviewAsync(
            int analysisId, int adminId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, adminId);

            var analysis = await _analyses
                .GetByIdAsync(analysisId);

            if (analysis == null)
                throw new KeyNotFoundException(
                    "AI analysis not found.");

            if (analysis.IsReviewed)
                throw new ValidationException(
                    "Analysis has already been reviewed.");

            analysis.IsReviewed = true;
            analysis.ReviewedById = adminId;
            analysis.ReviewedAt = DateTime.UtcNow;

            await _analyses.UpdateAsync(analysis);
        }
    }
}
