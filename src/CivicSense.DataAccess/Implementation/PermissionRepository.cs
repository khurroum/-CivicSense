
using CivicSense.DataAccess.Interface;
using Microsoft.EntityFrameworkCore;

namespace CivicSense.DataAccess.Implementation
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly ApplicationDbContext _context;

        public PermissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<bool> HasPermissionAsync(
            int userId, string permissionCode)
        {
            // Evaluates effective permissions in one SQL query.
            // An active individual deny overrides all grants.

            return _context.Permissions
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Name == permissionCode &&
                    p.IsActive &&

                    _context.Users.Any(u =>
                        u.Id == userId &&
                        u.IsActive) &&

                    !_context.UserPermissions.Any(up =>
                        up.UserId == userId &&
                        up.PermissionId == p.Id &&
                        up.IsActive &&
                        !up.IsAllowed) &&

                    (
                        // Direct permission grant.
                        _context.UserPermissions.Any(up =>
                            up.UserId == userId &&
                            up.PermissionId == p.Id &&
                            up.IsActive &&
                            up.IsAllowed)

                        ||

                        // Permission inherited from a group.
                        _context.PermissionGroupMaps.Any(m =>
                            m.PermissionId == p.Id &&
                            m.IsActive &&

                            _context.PermissionGroups.Any(g =>
                                g.Id == m.PermissionGroupId &&
                                g.IsActive &&

                                (
                                    // Default role group.
                                    _context.Users.Any(u =>
                                        u.Id == userId &&
                                        u.IsActive &&
                                        g.DefaultRoleId == u.Role)

                                    ||

                                    // Explicit user group membership.
                                    _context.UserPermissions.Any(up =>
                                        up.UserId == userId &&
                                        up.PermissionGroupId == g.Id &&
                                        up.IsActive &&
                                        up.IsAllowed)
                                )
                            )
                        )
                    ));
        }
    }
}
