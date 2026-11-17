using CivicSense.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace CivicSense.DataAccess.Interface
{
    public interface IAccessAdminRepository
    {
        Task<List<User>> GetUsersAsync();

        Task<User?> GetUserAsync(
            int id, bool tracking = false);

        Task<bool> IsActiveAdminAsync(int userId);

        Task<bool> EmailExistsAsync(
            string email, int excludedUserId);

        Task<bool> HasOpenWorkAsync(int userId);

        Task<List<Permission>> GetPermissionsAsync();

        Task<List<PermissionGroup>> GetGroupsAsync();

        Task<PermissionGroup?> GetGroupAsync(
            int id, bool tracking = false);

        Task<bool> GroupNameExistsAsync(
            string name, int excludedGroupId);

        Task<List<PermissionGroupMap>> GetMapsAsync(
            int? groupId = null,
            bool tracking = false);

        Task<List<UserPermission>> GetGrantsAsync(
            int userId, bool tracking = false);

        void Add<T>(T entity) where T : class;

        Task<int> SaveAsync();

        Task<IDbContextTransaction> BeginWriteAsync();
    }
}