
using System.ComponentModel.DataAnnotations;
using CivicSense.Entities;

namespace CivicSense.Facade.DTOs
{
    public class IssueCreateDto
    {
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(5000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        public byte IssueType { get; set; }
            = IssueTypeLookup.PublicCivic;

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }
    }

    public class IssueUpdateDto
    {
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(5000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        // Original SQL Server concurrency token.
        [Required]
        [MinLength(8)]
        [MaxLength(8)]
        public byte[] RowVersion { get; set; }
            = Array.Empty<byte>();
    }

    public class IssueDto
    {
        public int Id { get; set; }

        public int ReporterId { get; set; }

        public int CategoryId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public byte IssueType { get; set; }

        public string IssueTypeName => IssueTypeLookup.NameOf(IssueType);

        public byte Status { get; set; }

        public string StatusName => IssueStatusLookup.NameOf(Status);

        // Populated only for the authorized report owner in issue details.
        public string? RejectionReason { get; set; }

        public byte Priority { get; set; }

        public string PriorityName => IssuePriorityLookup.NameOf(Priority);

        public DateTime CreatedAt { get; set; }

        // Used when editing an issue.
        public byte[] RowVersion { get; set; }
            = Array.Empty<byte>();
    }
}
