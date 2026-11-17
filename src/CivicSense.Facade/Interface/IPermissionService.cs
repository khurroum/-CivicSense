namespace CivicSense.Facade.Interface
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(
            int actorId, string permissionCode);

        Task RequireAsync(
            int actorId, string permissionCode);
    }
}