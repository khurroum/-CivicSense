
using System;

namespace CivicSense.Entities
{
    public class AIAnalysis
    {
        public int Id { get; set; }

        public int IssueId { get; set; }
        public virtual Issue Issue { get; set; } = null!;

        public string Provider { get; set; } = string.Empty;

        public string ModelName { get; set; } = string.Empty;

        public IssueType? SuggestedIssueType { get; set; }

        public int? SuggestedCategoryId { get; set; }
        public virtual Category? SuggestedCategory { get; set; }

        public IssuePriority? SuggestedPriority { get; set; }

        public string? SuggestedSummary { get; set; }

        public string? Explanation { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public bool IsReviewed { get; set; } = false;

        public int? ReviewedById { get; set; }
        public virtual User? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}
