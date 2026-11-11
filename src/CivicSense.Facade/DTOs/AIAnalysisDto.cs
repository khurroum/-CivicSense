
using System;
using System.ComponentModel.DataAnnotations;
using CivicSense.Entities;

namespace CivicSense.Facade.DTOs
{
    public class AIAnalysisCreateDto
    {
        [Range(1, int.MaxValue)]
        public int IssueId { get; set; }

        [Required]
        [StringLength(100)]
        public string Provider { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ModelName { get; set; } = string.Empty;

        public IssueType? SuggestedIssueType { get; set; }

        public int? SuggestedCategoryId { get; set; }

        public IssuePriority? SuggestedPriority { get; set; }

        [StringLength(2000)]
        public string? SuggestedSummary { get; set; }

        [StringLength(4000)]
        public string? Explanation { get; set; }

        [Range(typeof(decimal), "0", "1")]
        public decimal? ConfidenceScore { get; set; }
    }

    public class AIAnalysisDto
    {
        public int Id { get; set; }

        public int IssueId { get; set; }

        public IssueType? SuggestedIssueType { get; set; }

        public int? SuggestedCategoryId { get; set; }

        public IssuePriority? SuggestedPriority { get; set; }

        public string? SuggestedSummary { get; set; }

        public string? Explanation { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public bool IsReviewed { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
