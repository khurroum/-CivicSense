using System.ComponentModel.DataAnnotations;
using CivicSense.Entities;

namespace CivicSense.Facade.DTOs
{
    public class AccessUserDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public byte Role { get; set; }

        public string RoleName =>
            UserRoleLookup.NameOf(Role);

        public bool IsActive { get; set; }
    }

    public class AccessGroupDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Tag { get; set; }

        public byte? DefaultRoleId { get; set; }

        public bool IsActive { get; set; }
    }

    public class GroupAdminInputDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Tag { get; set; }

        public bool IsActive { get; set; } = true;

        public int[] PermissionIds { get; set; }
            = Array.Empty<int>();

        [StringLength(64)]
        public string? Version { get; set; }
    }

    public class UserAdminInputDto
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Range(1, 3)]
        public byte Role { get; set; }
            = UserRoleLookup.Resident;

        public bool IsActive { get; set; } = true;

        // Used only when creating a new account.
        [StringLength(128, MinimumLength = 12)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare(nameof(Password),
            ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        public int[] GroupIds { get; set; }
            = Array.Empty<int>();

        public int[] AllowIds { get; set; }
            = Array.Empty<int>();

        public int[] DenyIds { get; set; }
            = Array.Empty<int>();

        [StringLength(64)]
        public string? Version { get; set; }
    }

    public class PermissionOptionDto
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool IsReserved { get; set; }

        public bool IsSelected { get; set; }

        public string DirectState { get; set; } = "Inherit";

        public string InheritedFrom { get; set; } = string.Empty;

        public bool EffectiveAllowed { get; set; }
    }

    public class GroupOptionDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool IsDefaultGroup { get; set; }

        public bool IsInherited { get; set; }

        public bool IsSelected { get; set; }
    }

    public class GroupAdminPageDto
    {
        public int Id { get; set; }

        public byte? DefaultRoleId { get; set; }

        public bool IsProtected =>
            DefaultRoleId == UserRoleLookup.Admin;

        public GroupAdminInputDto Form { get; set; }
            = new();

        public List<PermissionOptionDto> Permissions
        { get; set; } = new();
    }

    public class UserAdminPageDto
    {
        public int Id { get; set; }

        public bool CanEdit { get; set; } = true;

        public UserAdminInputDto Form { get; set; }
            = new();

        public List<GroupOptionDto> Groups
        { get; set; } = new();

        public List<PermissionOptionDto> Permissions
        { get; set; } = new();
    }
}