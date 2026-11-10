
namespace CivicSense.Entities
{
    public enum IssueStatus : byte
    {
        Reported = 1,
        UnderReview = 2,
        Assigned = 3,
        InProgress = 4,
        Resolved = 5,
        Rejected = 6,
        NeedsInformation = 7,
        Reopened = 8
    }
}
