
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;

namespace CivicSense.Facade.Implementation
{
    public class AIAnalysisService : IAIAnalysisService
    {
        private readonly IAIAnalysisRepository _analyses;
        private readonly IIssueRepository _issues;
        private readonly ICategoryRepository _categories;
        private readonly IUserRepository _users;

        public AIAnalysisService(
            IAIAnalysisRepository analyses,
            IIssueRepository issues,
            ICategoryRepository categories,
            IUserRepository users)
        {
            _analyses = analyses;
            _issues = issues;
            _categories = categories;
            _users = users;
        }

        // Only a trusted server-side AI integration
        // should call this method.
        public async Task<AIAnalysisDto> RecordSuggestionAsync(
            AIAnalysisCreateDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            FacadeValidation.Validate(dto);

            var issue = await _issues.GetByIdAsync(dto.IssueId);

            if (issue == null || issue.IsArchived)
            {
                throw new ValidationException(
                    "A valid issue is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Provider) ||
                string.IsNullOrWhiteSpace(dto.ModelName))
            {
                throw new ValidationException(
                    "AI provider and model name are required.");
            }

            if (dto.SuggestedIssueType.HasValue &&
                !Enum.IsDefined(dto.SuggestedIssueType.Value))
            {
                throw new ValidationException(
                    "Invalid suggested issue type.");
            }

            if (dto.SuggestedPriority.HasValue &&
                !Enum.IsDefined(dto.SuggestedPriority.Value))
            {
                throw new ValidationException(
                    "Invalid suggested priority.");
            }

            if (dto.ConfidenceScore.HasValue &&
                (dto.ConfidenceScore < 0 ||
                 dto.ConfidenceScore > 1))
            {
                throw new ValidationException(
                    "Confidence must be between 0 and 1.");
            }

            if (dto.SuggestedCategoryId.HasValue)
            {
                var category = await _categories.GetByIdAsync(
                    dto.SuggestedCategoryId.Value);

                if (category == null || !category.IsActive)
                {
                    throw new ValidationException(
                        "Suggested category is invalid.");
                }
            }

            var analysis = new AIAnalysis
            {
                IssueId = dto.IssueId,
                Provider = dto.Provider.Trim(),
                ModelName = dto.ModelName.Trim(),
                SuggestedIssueType =
                    dto.SuggestedIssueType,
                SuggestedCategoryId =
                    dto.SuggestedCategoryId,
                SuggestedPriority =
                    dto.SuggestedPriority,
                SuggestedSummary =
                    dto.SuggestedSummary?.Trim(),
                Explanation = dto.Explanation?.Trim(),
                ConfidenceScore = dto.ConfidenceScore,
                IsReviewed = false,
                ReviewedById = null,
                ReviewedAt = null,
                CreatedAt = DateTime.UtcNow
            };

            await _analyses.AddAsync(analysis);

            return Map(analysis);
        }

        public async Task ReviewAsync(
            int analysisId, int adminId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, adminId);

            var analysis = await _analyses
                .GetByIdAsync(analysisId);

            if (analysis == null)
            {
                throw new KeyNotFoundException(
                    "AI analysis not found.");
            }

            if (analysis.IsReviewed)
            {
                throw new ValidationException(
                    "This analysis has already been reviewed.");
            }

            analysis.IsReviewed = true;
            analysis.ReviewedById = adminId;
            analysis.ReviewedAt = DateTime.UtcNow;

            await _analyses.UpdateAsync(analysis);
        }

        private static AIAnalysisDto Map(
            AIAnalysis analysis)
        {
            return new AIAnalysisDto
            {
                Id = analysis.Id,
                IssueId = analysis.IssueId,
                SuggestedIssueType =
                    analysis.SuggestedIssueType,
                SuggestedCategoryId =
                    analysis.SuggestedCategoryId,
                SuggestedPriority =
                    analysis.SuggestedPriority,
                SuggestedSummary =
                    analysis.SuggestedSummary,
                Explanation = analysis.Explanation,
                ConfidenceScore =
                    analysis.ConfidenceScore,
                IsReviewed = analysis.IsReviewed,
                CreatedAt = analysis.CreatedAt
            };
        }
    }
}
