namespace CivicSense.Entities
{
    public class UserRoleLookup
    {
        public byte Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public const byte Resident = 1;
        public const byte Staff = 2;
        public const byte Admin = 3;

        public static bool IsDefined(byte id) =>
            id is Resident or Staff or Admin;

        public static string NameOf(byte? id) => id switch
        {
            Resident => nameof(Resident),
            Staff => nameof(Staff),
            Admin => nameof(Admin),
            null => "Not specified",
            _ => "Unknown"
        };
    }

    public class IssueTypeLookup
    {
        public byte Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public const byte PublicCivic = 1;
        public const byte PersonalPrivate = 2;

        public static bool IsDefined(byte id) =>
            id is PublicCivic or PersonalPrivate;

        public static string NameOf(byte? id) => id switch
        {
            PublicCivic => nameof(PublicCivic),
            PersonalPrivate => nameof(PersonalPrivate),
            null => "Not specified",
            _ => "Unknown"
        };
    }

    public class IssueStatusLookup
    {
        public byte Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public const byte Reported = 1;
        public const byte UnderReview = 2;
        public const byte Assigned = 3;
        public const byte InProgress = 4;
        public const byte Resolved = 5;
        public const byte Rejected = 6;
        public const byte NeedsInformation = 7;
        public const byte Reopened = 8;

        public static bool IsDefined(byte id) =>
            id is Reported or UnderReview or Assigned or InProgress or Resolved or Rejected or NeedsInformation or Reopened;

        public static string NameOf(byte? id) => id switch
        {
            Reported => nameof(Reported),
            UnderReview => nameof(UnderReview),
            Assigned => nameof(Assigned),
            InProgress => nameof(InProgress),
            Resolved => nameof(Resolved),
            Rejected => nameof(Rejected),
            NeedsInformation => nameof(NeedsInformation),
            Reopened => nameof(Reopened),
            null => "Not specified",
            _ => "Unknown"
        };
    }

    public class IssuePriorityLookup
    {
        public byte Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public const byte Low = 1;
        public const byte Medium = 2;
        public const byte High = 3;
        public const byte Critical = 4;

        public static bool IsDefined(byte id) =>
            id is Low or Medium or High or Critical;

        public static string NameOf(byte? id) => id switch
        {
            Low => nameof(Low),
            Medium => nameof(Medium),
            High => nameof(High),
            Critical => nameof(Critical),
            null => "Not specified",
            _ => "Unknown"
        };
    }

    public class ServiceRequestStatusLookup
    {
        public byte Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public const byte Pending = 1;
        public const byte Accepted = 2;
        public const byte InProgress = 3;
        public const byte Completed = 4;
        public const byte Cancelled = 5;
        public const byte Rejected = 6;

        public static bool IsDefined(byte id) =>
            id is Pending or Accepted or InProgress or Completed or Cancelled or Rejected;

        public static string NameOf(byte? id) => id switch
        {
            Pending => nameof(Pending),
            Accepted => nameof(Accepted),
            InProgress => nameof(InProgress),
            Completed => nameof(Completed),
            Cancelled => nameof(Cancelled),
            Rejected => nameof(Rejected),
            null => "Not specified",
            _ => "Unknown"
        };
    }

    public class PaymentStatusLookup
    {
        public byte Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public const byte Pending = 1;
        public const byte Processing = 2;
        public const byte Completed = 3;
        public const byte Failed = 4;
        public const byte Refunded = 5;
        public const byte Cancelled = 6;

        public static bool IsDefined(byte id) =>
            id is Pending or Processing or Completed or Failed or Refunded or Cancelled;

        public static string NameOf(byte? id) => id switch
        {
            Pending => nameof(Pending),
            Processing => nameof(Processing),
            Completed => nameof(Completed),
            Failed => nameof(Failed),
            Refunded => nameof(Refunded),
            Cancelled => nameof(Cancelled),
            null => "Not specified",
            _ => "Unknown"
        };
    }

}
