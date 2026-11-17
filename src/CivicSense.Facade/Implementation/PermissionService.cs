using CivicSense.DataAccess.Interface;
using CivicSense.Facade.Interface;

namespace CivicSense.Facade.Implementation
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissions;

        public PermissionService(
            IPermissionRepository permissions)
        {
            _permissions = permissions;
        }

        public Task<bool> HasPermissionAsync(
            int actorId, string permissionCode)
        {
            if (actorId <= 0 ||
                string.IsNullOrWhiteSpace(permissionCode))
            {
                return Task.FromResult(false);
            }

            return _permissions.HasPermissionAsync(
                actorId, permissionCode);
        }

        public async Task RequireAsync(
            int actorId, string permissionCode)
        {
            if (!await HasPermissionAsync(
                actorId, permissionCode))
            {
                throw new UnauthorizedAccessException(
                    "Required permission is missing.");
            }
        }
    }
}
