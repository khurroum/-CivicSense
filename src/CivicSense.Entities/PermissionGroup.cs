namespace CivicSense.Entities
{
    public class PermissionGroup
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Tag { get; set; }

        // Automatic membership for an existing base role.
        public byte? DefaultRoleId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}