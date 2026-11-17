using CivicSense.Facade.DTOs;

namespace CivicSense.Facade.Interface
{
    public interface IAccessAdminService
    {
        Task<List<AccessUserDto>> GetUsersAsync(
            int actorId);

        Task<List<AccessGroupDto>> GetGroupsAsync(
            int actorId);

        Task<GroupAdminPageDto> GetGroupPageAsync(
            int id, int actorId);

        Task<UserAdminPageDto> GetUserPageAsync(
            int id, int actorId);

        Task<int> SaveGroupAsync(
            int id,
            GroupAdminInputDto dto,
            int actorId);

        Task<int> SaveUserAsync(
            int id,
            UserAdminInputDto dto,
            int actorId);
    }
}