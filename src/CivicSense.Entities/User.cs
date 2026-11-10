
using System;
using System.Collections.Generic;

namespace CivicSense.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? ProfileImagePath { get; set; }

        public UserRole Role { get; set; } = UserRole.Resident;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<Issue> Issues { get; set; }
            = new List<Issue>();

        public virtual ICollection<IssueAssignment> StaffAssignments { get; set; }
            = new List<IssueAssignment>();

        public virtual ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();

        public virtual ICollection<Feedback> Feedbacks { get; set; }
            = new List<Feedback>();

        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; }
            = new List<ServiceRequest>();
    }
}
