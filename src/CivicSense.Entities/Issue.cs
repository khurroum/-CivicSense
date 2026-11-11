
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CivicSense.Entities
{
    public class Issue
    {
        public int Id { get; set; }

        public int ReporterId { get; set; }
        public virtual User Reporter { get; set; } = null!;

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public IssueType IssueType { get; set; }
            = IssueType.PublicCivic;

        public IssueStatus Status { get; set; }
            = IssueStatus.Reported;

        public IssuePriority Priority { get; set; }
            = IssuePriority.Medium;

        public bool IsArchived { get; set; } = false;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        // SQL Server concurrency protection.
        // Generated and updated automatically by the database.
        [Timestamp]
        public byte[] RowVersion { get; set; }
            = Array.Empty<byte>();

        // Navigation Properties
        public virtual ICollection<IssueImage> Images
        { get; set; } = new List<IssueImage>();

        public virtual ICollection<IssueAssignment> Assignments
        { get; set; } = new List<IssueAssignment>();

        public virtual ICollection<IssueStatusHistory> StatusHistories
        { get; set; } = new List<IssueStatusHistory>();

        public virtual ICollection<IssueComment> Comments
        { get; set; } = new List<IssueComment>();

        public virtual ICollection<IssueConfirmation> Confirmations
        { get; set; } = new List<IssueConfirmation>();

        public virtual ICollection<AIAnalysis> AIAnalyses
        { get; set; } = new List<AIAnalysis>();

        public virtual ICollection<Feedback> Feedbacks
        { get; set; } = new List<Feedback>();

        public virtual ICollection<ServiceRequest> ServiceRequests
        { get; set; } = new List<ServiceRequest>();
    }
}
