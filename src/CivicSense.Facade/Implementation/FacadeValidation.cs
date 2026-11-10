
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;

namespace CivicSense.Facade.Implementation
{
    internal static class FacadeValidation
    {
        public static void Validate<T>(T model)
            where T : class
        {
            var results = new List<ValidationResult>();

            var context = new ValidationContext(model);

            bool isValid = Validator.TryValidateObject(
                model,
                context,
                results,
                validateAllProperties: true);

            if (!isValid)
            {
                throw new ValidationException(
                    string.Join("; ",
                        results.Select(x => x.ErrorMessage)));
            }
        }

        public static async Task<User> GetActiveUserAsync(
            IUserRepository users, int actorId)
        {
            if (actorId <= 0)
            {
                throw new UnauthorizedAccessException(
                    "Authentication is required.");
            }

            var user = await users.GetByIdAsync(actorId);

            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "User not found or account inactive.");
            }

            return user;
        }

        public static async Task RequireAdminAsync(
            IUserRepository users, int actorId)
        {
            var user = await GetActiveUserAsync(
                users, actorId);

            if (user.Role != UserRole.Admin)
            {
                throw new UnauthorizedAccessException(
                    "Administrator access is required.");
            }
        }
    }
}
