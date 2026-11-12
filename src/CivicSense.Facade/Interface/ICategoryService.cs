
using CivicSense.Facade.DTOs;

namespace CivicSense.Facade.Interface
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();

        Task<List<CategoryDto>> GetActiveAsync();

        Task<CategoryDto?> GetByIdAsync(int id);

        Task<CategoryDto> CreateAsync(
            CategoryCreateDto dto, int actorId);

        Task<bool> UpdateAsync(
            int id,
            CategoryUpdateDto dto,
            int actorId);

        Task<bool> DeactivateAsync(
            int id, int actorId);

        Task<bool> ReactivateAsync(
            int id, int actorId);
    }
}
