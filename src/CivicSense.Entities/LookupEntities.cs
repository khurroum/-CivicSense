
namespace CivicSense.Entities
{
    public class UserRoleLookup
    {
        public UserRole Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class IssueTypeLookup
    {
        public IssueType Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class IssueStatusLookup
    {
        public IssueStatus Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class IssuePriorityLookup
    {
        public IssuePriority Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ServiceRequestStatusLookup
    {
        public ServiceRequestStatus Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PaymentStatusLookup
    {
        public PaymentStatus Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

}
