
using System;

namespace CivicSense.Entities
{
    public class PaymentTransaction
    {
        public int Id { get; set; }

        public int ServiceRequestId { get; set; }
        public virtual ServiceRequest ServiceRequest
        { get; set; } = null!;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "BDT";

        public string PaymentMethod { get; set; } = string.Empty;

        public string? GatewayTransactionId { get; set; }

        public PaymentStatus Status { get; set; }
            = PaymentStatus.Pending;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }
    }
}
