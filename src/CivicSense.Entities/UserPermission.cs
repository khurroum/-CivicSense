namespace CivicSense.Entities
{
    public class UserPermission
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? PermissionId { get; set; }

        public int? PermissionGroupId { get; set; }

        // Exactly one target is required.
        // Direct denies override all grants.
        public bool IsAllowed { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }
}