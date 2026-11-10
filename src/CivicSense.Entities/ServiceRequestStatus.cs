
namespace CivicSense.Entities
{
    public enum ServiceRequestStatus : byte
    {
        Pending = 1,
        Accepted = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5,
        Rejected = 6
    }
}
