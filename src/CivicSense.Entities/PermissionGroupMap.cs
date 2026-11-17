namespace CivicSense.Entities
{
    public class PermissionGroupMap
    {
        public int Id { get; set; }

        public int PermissionGroupId { get; set; }

        public int PermissionId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}