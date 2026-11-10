
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;
using System.ComponentModel.DataAnnotations;

namespace CivicSense.Facade.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categories;
        private readonly IUserRepository _users;

        public CategoryService(
            ICategoryRepository categories,
            IUserRepository users)
        {
            _categories = categories;
            _users = users;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var categories = await _categories.GetAllAsync();

            return categories
                .Select(Map)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public async Task<List<CategoryDto>> GetActiveAsync()
        {
            var categories = await _categories.GetActiveAsync();

            return categories.Select(Map).ToList();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _categories.GetByIdAsync(id);

            return category == null ? null : Map(category);
        }

        public async Task<CategoryDto> CreateAsync(
            CategoryCreateDto dto, int actorId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, actorId);

            FacadeValidation.Validate(dto);

            string name = dto.Name.Trim();

            if (name.Length < 2)
            {
                throw new ValidationException(
                    "Category name must have at least 2 characters.");
            }

            var categories = await _categories.GetAllAsync();

            bool exists = categories.Any(x =>
                string.Equals(x.Name, name,
                    StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                throw new ValidationException(
                    "A category with this name already exists.");
            }

            var category = new Category
            {
                Name = name,
                Description = dto.Description?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _categories.AddAsync(category);

            return Map(category);
        }

        public async Task<bool> UpdateAsync(
            int id, CategoryUpdateDto dto, int actorId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, actorId);

            FacadeValidation.Validate(dto);

            var category = await _categories.GetByIdAsync(id);

            if (category == null)
                return false;

            string name = dto.Name.Trim();

            if (name.Length < 2)
            {
                throw new ValidationException(
                    "Category name must have at least 2 characters.");
            }

            var categories = await _categories.GetAllAsync();

            bool duplicate = categories.Any(x =>
                x.Id != id &&
                string.Equals(x.Name, name,
                    StringComparison.OrdinalIgnoreCase));

            if (duplicate)
            {
                throw new ValidationException(
                    "A category with this name already exists.");
            }

            category.Name = name;
            category.Description = dto.Description?.Trim();

            await _categories.UpdateAsync(category);

            return true;
        }

        public async Task<bool> DeactivateAsync(
            int id, int actorId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, actorId);

            var category = await _categories.GetByIdAsync(id);

            if (category == null)
                return false;

            category.IsActive = false;

            await _categories.UpdateAsync(category);

            return true;
        }

        private static CategoryDto Map(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            };
        }
    }
}
