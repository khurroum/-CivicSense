using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.Json;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;
using CivicSense.Framework.Security;
using Microsoft.EntityFrameworkCore;

namespace CivicSense.Facade.Implementation
{
    public class AccessAdminService : IAccessAdminService
    {
        private readonly IAccessAdminRepository _repository;
        private readonly IPermissionService _permissions;

        public AccessAdminService(
            IAccessAdminRepository repository,
            IPermissionService permissions)
        {
            _repository = repository;
            _permissions = permissions;
        }

        // =========================================
        // ADMINISTRATION AUTHORIZATION
        // =========================================

        private async Task RequireManagerAsync(int actorId)
        {
            if (!await _repository.IsActiveAdminAsync(actorId))
            {
                throw new UnauthorizedAccessException(
                    "Active administrator access is required.");
            }

            await _permissions.RequireAsync(
                actorId,
                PermissionCodes.ManageAccess);
        }

        // =========================================
        // LIST USERS
        // =========================================

        public async Task<List<AccessUserDto>> GetUsersAsync(
            int actorId)
        {
            await RequireManagerAsync(actorId);

            var users = await _repository.GetUsersAsync();

            return users.Select(x => new AccessUserDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                Role = x.Role,
                IsActive = x.IsActive
            }).ToList();
        }

        // =========================================
        // LIST PERMISSION GROUPS
        // =========================================

        public async Task<List<AccessGroupDto>> GetGroupsAsync(
            int actorId)
        {
            await RequireManagerAsync(actorId);

            var groups = await _repository.GetGroupsAsync();

            return groups.Select(x => new AccessGroupDto
            {
                Id = x.Id,
                Name = x.Name,
                Tag = x.Tag,
                DefaultRoleId = x.DefaultRoleId,
                IsActive = x.IsActive
            }).ToList();
        }

        // =========================================
        // LOAD GROUP CREATE / EDIT PAGE
        // =========================================

        public async Task<GroupAdminPageDto> GetGroupPageAsync(
            int id, int actorId)
        {
            await RequireManagerAsync(actorId);

            if (id < 0)
                throw new KeyNotFoundException();

            var page = new GroupAdminPageDto
            {
                Id = id
            };

            var maps = new List<PermissionGroupMap>();

            if (id > 0)
            {
                var group = await _repository.GetGroupAsync(id)
                    ?? throw new KeyNotFoundException(
                        "Permission group not found.");

                maps = await _repository.GetMapsAsync(id);

                page.DefaultRoleId = group.DefaultRoleId;

                page.Form = new GroupAdminInputDto
                {
                    Name = group.Name,
                    Tag = group.Tag,
                    IsActive = group.IsActive,

                    PermissionIds = maps
                        .Where(x => x.IsActive)
                        .Select(x => x.PermissionId)
                        .ToArray(),

                    Version = GroupVersion(group, maps)
                };
            }

            var permissions =
                await _repository.GetPermissionsAsync();

            page.Permissions = permissions
                .Select(p => new PermissionOptionDto
                {
                    Id = p.Id,
                    Code = p.Name,
                    Label = p.DisplayText,
                    IsActive = p.IsActive,

                    IsReserved =
                        p.Name == PermissionCodes.ManageAccess,

                    IsSelected = maps.Any(m =>
                        m.PermissionId == p.Id &&
                        m.IsActive)
                })
                .ToList();

            return page;
        }

        // =========================================
        // LOAD USER CREATE / EDIT PAGE
        // =========================================

        public async Task<UserAdminPageDto> GetUserPageAsync(
            int id, int actorId)
        {
            await RequireManagerAsync(actorId);

            if (id < 0)
                throw new KeyNotFoundException();

            var page = new UserAdminPageDto
            {
                Id = id,
                CanEdit = id != actorId
            };

            var grants = new List<UserPermission>();

            if (id > 0)
            {
                var user = await _repository.GetUserAsync(id)
                    ?? throw new KeyNotFoundException(
                        "User not found.");

                grants = await _repository.GetGrantsAsync(id);

                page.Form = new UserAdminInputDto
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    Version = UserVersion(user, grants)
                };
            }

            var groups = await _repository.GetGroupsAsync();

            var permissions =
                await _repository.GetPermissionsAsync();

            var maps = await _repository.GetMapsAsync();

            // Explicit custom group membership.
            var explicitGroups = grants
                .Where(x =>
                    x.PermissionGroupId.HasValue &&
                    x.IsActive &&
                    x.IsAllowed)
                .Select(x => x.PermissionGroupId!.Value)
                .ToHashSet();

            page.Form.GroupIds = groups
                .Where(x =>
                    x.IsActive &&
                    !x.DefaultRoleId.HasValue &&
                    explicitGroups.Contains(x.Id))
                .Select(x => x.Id)
                .ToArray();

            page.Groups = groups
                .Where(g =>
                    !g.DefaultRoleId.HasValue ||
                    (id > 0 &&
                     (g.DefaultRoleId == page.Form.Role ||
                      explicitGroups.Contains(g.Id))))
                .Select(g => new GroupOptionDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsActive = g.IsActive,

                    IsDefaultGroup =
                        g.DefaultRoleId.HasValue,

                    IsInherited =
                        id > 0 &&
                        g.IsActive &&
                        g.DefaultRoleId == page.Form.Role,

                    IsSelected =
                        explicitGroups.Contains(g.Id)
                })
                .ToList();

            var effectiveGroups = groups
                .Where(g =>
                    id > 0 &&
                    g.IsActive &&
                    (
                        g.DefaultRoleId == page.Form.Role ||
                        explicitGroups.Contains(g.Id)
                    ))
                .ToList();

            foreach (var permission in permissions)
            {
                var direct = grants.FirstOrDefault(x =>
                    x.PermissionId == permission.Id &&
                    x.IsActive);

                var inheritedFrom = effectiveGroups
                    .Where(g => maps.Any(m =>
                        m.PermissionGroupId == g.Id &&
                        m.PermissionId == permission.Id &&
                        m.IsActive))
                    .Select(g => g.Name);

                bool effective = false;

                if (id > 0)
                {
                    effective =
                        await _permissions.HasPermissionAsync(
                            id, permission.Name);
                }

                // Management requires both the permission
                // and the existing Admin classification.
                if (permission.Name ==
                        PermissionCodes.ManageAccess &&
                    page.Form.Role != UserRoleLookup.Admin)
                {
                    effective = false;
                }

                page.Permissions.Add(new PermissionOptionDto
                {
                    Id = permission.Id,
                    Code = permission.Name,
                    Label = permission.DisplayText,
                    IsActive = permission.IsActive,

                    IsReserved =
                        permission.Name ==
                        PermissionCodes.ManageAccess,

                    DirectState = direct == null
                        ? "Inherit"
                        : direct.IsAllowed
                            ? "Allow"
                            : "Deny",

                    InheritedFrom =
                        string.Join(", ", inheritedFrom),

                    EffectiveAllowed = effective
                });
            }

            page.Form.AllowIds = page.Permissions
                .Where(p =>
                    p.IsActive &&
                    !p.IsReserved &&
                    p.DirectState == "Allow")
                .Select(p => p.Id)
                .ToArray();

            page.Form.DenyIds = page.Permissions
                .Where(p =>
                    p.IsActive &&
                    !p.IsReserved &&
                    p.DirectState == "Deny")
                .Select(p => p.Id)
                .ToArray();

            return page;
        }

        // =========================================
        // CREATE OR UPDATE PERMISSION GROUP
        // =========================================

        public async Task<int> SaveGroupAsync(
            int id,
            GroupAdminInputDto dto,
            int actorId)
        {
            // Recheck authorization within the transaction.
            await using var transaction =
                await _repository.BeginWriteAsync();

            await RequireManagerAsync(actorId);

            FacadeValidation.Validate(dto);

            if (id < 0)
                throw new KeyNotFoundException();

            string name = dto.Name.Trim();

            if (name.Length < 2)
            {
                throw new ValidationException(
                    "Group name must have at least 2 characters.");
            }

            if (await _repository.GroupNameExistsAsync(name, id))
            {
                throw new ValidationException(
                    "A permission group with this name already exists.");
            }

            var permissions =
                await _repository.GetPermissionsAsync();

            var allowedPermissionIds = permissions
                .Where(p =>
                    p.IsActive &&
                    p.Name != PermissionCodes.ManageAccess)
                .Select(p => p.Id)
                .ToHashSet();

            var selectedIds = ToSet(dto.PermissionIds);

            RequireSubset(
                selectedIds,
                allowedPermissionIds,
                "Invalid or protected permission selected.");

            var protectedIds = permissions
                .Where(p =>
                    p.Name == PermissionCodes.ManageAccess)
                .Select(p => p.Id)
                .ToHashSet();

            PermissionGroup group;

            var maps = new List<PermissionGroupMap>();

            if (id == 0)
            {
                group = new PermissionGroup
                {
                    Name = name,
                    Tag = dto.Tag?.Trim(),
                    IsActive = dto.IsActive,

                    // Custom groups have no default role.
                    DefaultRoleId = null
                };

                _repository.Add(group);

                await _repository.SaveAsync();
            }
            else
            {
                group = await _repository.GetGroupAsync(
                    id, tracking: true)
                    ?? throw new KeyNotFoundException(
                        "Permission group not found.");

                maps = await _repository.GetMapsAsync(
                    id, tracking: true);

                RequireVersion(
                    dto.Version,
                    GroupVersion(group, maps));

                if (group.DefaultRoleId ==
                        UserRoleLookup.Admin &&
                    !dto.IsActive)
                {
                    throw new ValidationException(
                        "The default Admin group cannot be deactivated.");
                }

                group.Name = name;
                group.Tag = dto.Tag?.Trim();
                group.IsActive = dto.IsActive;
            }

            // Reactivate or deactivate existing mappings.
            // Protected management mappings are untouched.
            foreach (var map in maps)
            {
                if (protectedIds.Contains(map.PermissionId))
                    continue;

                map.IsActive =
                    selectedIds.Contains(map.PermissionId);
            }

            // Add mappings that do not already exist.
            foreach (int permissionId in selectedIds)
            {
                if (maps.Any(x =>
                    x.PermissionId == permissionId))
                {
                    continue;
                }

                _repository.Add(new PermissionGroupMap
                {
                    PermissionGroupId = group.Id,
                    PermissionId = permissionId,
                    IsActive = true
                });
            }

            await _repository.SaveAsync();

            await transaction.CommitAsync();

            return group.Id;
        }

        // =========================================
        // CREATE OR UPDATE USER
        // =========================================

        public async Task<int> SaveUserAsync(
            int id,
            UserAdminInputDto dto,
            int actorId)
        {
            await using var transaction =
                await _repository.BeginWriteAsync();

            await RequireManagerAsync(actorId);

            FacadeValidation.Validate(dto);

            if (id < 0)
                throw new KeyNotFoundException();

            // Prevent accidental self-lockout.
            if (id == actorId)
            {
                throw new ValidationException(
                    "You cannot modify your own account here.");
            }

            string fullName = dto.FullName.Trim();

            string email =
                dto.Email.Trim().ToLowerInvariant();

            if (fullName.Length < 2)
            {
                throw new ValidationException(
                    "Full name must have at least 2 characters.");
            }

            if (!UserRoleLookup.IsDefined(dto.Role))
            {
                throw new ValidationException(
                    "Invalid account role.");
            }

            if (await _repository.EmailExistsAsync(email, id))
            {
                throw new ValidationException(
                    "An account with this email already exists.");
            }

            var groups = await _repository.GetGroupsAsync();

            var permissions =
                await _repository.GetPermissionsAsync();

            var allowedGroupIds = groups
                .Where(x =>
                    x.IsActive &&
                    !x.DefaultRoleId.HasValue)
                .Select(x => x.Id)
                .ToHashSet();

            var allowedPermissionIds = permissions
                .Where(x =>
                    x.IsActive &&
                    x.Name != PermissionCodes.ManageAccess)
                .Select(x => x.Id)
                .ToHashSet();

            var groupIds = ToSet(dto.GroupIds);
            var allowIds = ToSet(dto.AllowIds);
            var denyIds = ToSet(dto.DenyIds);

            RequireSubset(
                groupIds,
                allowedGroupIds,
                "Invalid or inactive permission group selected.");

            RequireSubset(
                allowIds,
                allowedPermissionIds,
                "Invalid or protected permission selected.");

            RequireSubset(
                denyIds,
                allowedPermissionIds,
                "Invalid or protected permission selected.");

            if (allowIds.Overlaps(denyIds))
            {
                throw new ValidationException(
                    "A permission cannot be allowed and denied simultaneously.");
            }

            User user;

            var grants = new List<UserPermission>();

            if (id == 0)
            {
                // Initial password is required for creation.
                if (string.IsNullOrEmpty(dto.Password) ||
                    dto.Password.Length < 12 ||
                    dto.Password.Length > 128)
                {
                    throw new ValidationException(
                        "Password must contain 12 to 128 characters.");
                }

                if (dto.Password != dto.ConfirmPassword)
                {
                    throw new ValidationException(
                        "Passwords do not match.");
                }

                user = new User
                {
                    FullName = fullName,
                    Email = email,

                    PasswordHash =
                        PasswordHelper.HashPassword(dto.Password),

                    Role = dto.Role,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _repository.Add(user);

                // Generate the new user ID before assigning
                // groups and direct permissions.
                await _repository.SaveAsync();
            }
            else
            {
                user = await _repository.GetUserAsync(
                    id, tracking: true)
                    ?? throw new KeyNotFoundException(
                        "User not found.");

                grants = await _repository.GetGrantsAsync(
                    id, tracking: true);

                RequireVersion(
                    dto.Version,
                    UserVersion(user, grants));

                // Existing base roles are not changed here.
                // Role-specific business obligations may exist.
                if (dto.Role != user.Role)
                {
                    throw new ValidationException(
                        "Changing an existing base role is not supported here.");
                }

                if (!string.IsNullOrEmpty(dto.Password) ||
                    !string.IsNullOrEmpty(dto.ConfirmPassword))
                {
                    throw new ValidationException(
                        "Password changes require a separate secure process.");
                }

                if (user.IsActive &&
                    !dto.IsActive &&
                    await _repository.HasOpenWorkAsync(user.Id))
                {
                    throw new ValidationException(
                        "This account has active work or an active service provider profile.");
                }

                user.FullName = fullName;
                user.Email = email;
                user.IsActive = dto.IsActive;
                user.UpdatedAt = DateTime.UtcNow;
            }

            var protectedIds = permissions
                .Where(x =>
                    x.Name == PermissionCodes.ManageAccess)
                .Select(x => x.Id)
                .ToHashSet();

            // Update existing user assignments.
            foreach (var grant in grants)
            {
                if (grant.PermissionId.HasValue)
                {
                    int permissionId =
                        grant.PermissionId.Value;

                    if (protectedIds.Contains(permissionId))
                        continue;

                    bool allow = allowIds.Contains(permissionId);
                    bool deny = denyIds.Contains(permissionId);

                    grant.IsActive = allow || deny;

                    if (grant.IsActive)
                        grant.IsAllowed = allow;
                }
                else if (grant.PermissionGroupId.HasValue)
                {
                    grant.IsActive = groupIds.Contains(
                        grant.PermissionGroupId.Value);

                    grant.IsAllowed = true;
                }
            }

            // Add new custom group memberships.
            foreach (int groupId in groupIds)
            {
                if (grants.Any(x =>
                    x.PermissionGroupId == groupId))
                {
                    continue;
                }

                _repository.Add(new UserPermission
                {
                    UserId = user.Id,
                    PermissionGroupId = groupId,
                    PermissionId = null,
                    IsAllowed = true,
                    IsActive = true
                });
            }

            // Add new direct permission assignments.
            foreach (int permissionId in
                allowIds.Concat(denyIds))
            {
                if (grants.Any(x =>
                    x.PermissionId == permissionId))
                {
                    continue;
                }

                _repository.Add(new UserPermission
                {
                    UserId = user.Id,
                    PermissionId = permissionId,
                    PermissionGroupId = null,
                    IsAllowed = allowIds.Contains(permissionId),
                    IsActive = true
                });
            }

            await _repository.SaveAsync();

            await transaction.CommitAsync();

            return user.Id;
        }

        // =========================================
        // VALIDATION HELPERS
        // =========================================

        private static HashSet<int> ToSet(int[]? values)
        {
            return (values ?? Array.Empty<int>())
                .ToHashSet();
        }

        private static void RequireSubset(
            HashSet<int> selected,
            HashSet<int> permitted,
            string message)
        {
            if (!selected.IsSubsetOf(permitted))
            {
                throw new ValidationException(message);
            }
        }

        // =========================================
        // STALE FORM PROTECTION
        // =========================================

        private static string Stamp(object value)
        {
            byte[] bytes =
                JsonSerializer.SerializeToUtf8Bytes(value);

            return Convert.ToHexString(
                SHA256.HashData(bytes));
        }

        private static void RequireVersion(
            string? submitted,
            string current)
        {
            if (string.IsNullOrEmpty(submitted) ||
                !string.Equals(
                    submitted,
                    current,
                    StringComparison.Ordinal))
            {
                throw new DbUpdateConcurrencyException(
                    "The record changed after this form was opened.");
            }
        }

        private static string GroupVersion(
            PermissionGroup group,
            List<PermissionGroupMap> maps)
        {
            return Stamp(new
            {
                group.Id,
                group.Name,
                group.Tag,
                group.DefaultRoleId,
                group.IsActive,

                Maps = maps
                    .OrderBy(x => x.Id)
                    .Select(x => new
                    {
                        x.Id,
                        x.PermissionId,
                        x.IsActive
                    })
                    .ToArray()
            });
        }

        private static string UserVersion(
            User user,
            List<UserPermission> grants)
        {
            return Stamp(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.IsActive,

                Grants = grants
                    .OrderBy(x => x.Id)
                    .Select(x => new
                    {
                        x.Id,
                        x.PermissionId,
                        x.PermissionGroupId,
                        x.IsAllowed,
                        x.IsActive
                    })
                    .ToArray()
            });
        }
    }
}