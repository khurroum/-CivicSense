
using System;
using System.ComponentModel.DataAnnotations;

namespace CivicSense.Facade.DTOs
{
    public class CommentCreateDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;

        public bool IsInternal { get; set; } = false;
    }

    public class CommentDto
    {
        public int Id { get; set; }

        public int IssueId { get; set; }

        public int AuthorId { get; set; }

        public string Content { get; set; } = string.Empty;

        public bool IsInternal { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class ConfirmationDto
    {
        public int IssueId { get; set; }

        public bool IsConfirmed { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class FeedbackCreateDto
    {
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(2000)]
        public string? Comment { get; set; }

        public bool IsResolutionConfirmed { get; set; }
    }

    public class FeedbackDto
    {
        public int Id { get; set; }

        public int IssueId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public bool IsResolutionConfirmed { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
