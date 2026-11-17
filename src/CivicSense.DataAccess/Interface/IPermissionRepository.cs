namespace CivicSense.DataAccess.Interface
{
    public interface IPermissionRepository
    {
        Task<bool> HasPermissionAsync(
            int userId, string permissionCode);
    }
}