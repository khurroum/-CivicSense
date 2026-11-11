
using System;
using System.ComponentModel.DataAnnotations;
using CivicSense.Entities;

namespace CivicSense.Facade.DTOs
{
    public class ServiceProviderCreateDto
    {
        [Required]
        [StringLength(150)]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ServiceArea { get; set; } = string.Empty;
    }

    public class ServiceProviderDto
    {
        public int Id { get; set; }

        public string BusinessName { get; set; } = string.Empty;

        public string ServiceType { get; set; } = string.Empty;

        public string ServiceArea { get; set; } = string.Empty;

        public bool IsVerified { get; set; }
    }

    public class ServiceRequestCreateDto
    {
        [Range(1, int.MaxValue)]
        public int ServiceProviderId { get; set; }

        public int? IssueId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ServiceAddress { get; set; } = string.Empty;
    }

    public class ServiceRequestDto
    {
        public int Id { get; set; }

        public int ServiceProviderId { get; set; }

        public int? IssueId { get; set; }

        public ServiceRequestStatus Status { get; set; }

        public decimal? EstimatedCost { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class PaymentTransactionDto
    {
        public int Id { get; set; }

        public int ServiceRequestId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;

        public PaymentStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}
