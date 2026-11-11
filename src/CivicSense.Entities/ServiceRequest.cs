
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CivicSense.Entities
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int ServiceProviderId { get; set; }
        public virtual ServiceProvider ServiceProvider
        { get; set; } = null!;

        public int? IssueId { get; set; }
        public virtual Issue? Issue { get; set; }

        public string Description { get; set; } = string.Empty;

        public string ServiceAddress { get; set; } = string.Empty;

        public DateTime? ScheduledAt { get; set; }

        public decimal? EstimatedCost { get; set; }

        public ServiceRequestStatus Status { get; set; }
            = ServiceRequestStatus.Pending;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Database-generated concurrency token
        [Timestamp]
        public byte[] RowVersion { get; set; }
            = Array.Empty<byte>();

        // Navigation Properties
        public virtual ICollection<PaymentTransaction> Payments
        { get; set; } = new List<PaymentTransaction>();
    }
}
