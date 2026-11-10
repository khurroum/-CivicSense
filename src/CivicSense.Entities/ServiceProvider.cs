
using System;
using System.Collections.Generic;

namespace CivicSense.Entities
{
    public class ServiceProvider
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public string BusinessName { get; set; } = string.Empty;

        public string ServiceType { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string ContactNumber { get; set; } = string.Empty;

        public string ServiceArea { get; set; } = string.Empty;

        public decimal? StartingPrice { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<ServiceRequest> ServiceRequests
        { get; set; } = new List<ServiceRequest>();
    }
}
