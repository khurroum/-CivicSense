
namespace CivicSense.Entities
{
    public enum PaymentStatus : byte
    {
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4,
        Refunded = 5,
        Cancelled = 6
    }
}
