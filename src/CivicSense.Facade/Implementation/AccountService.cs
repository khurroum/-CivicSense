
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;
using CivicSense.Framework.Security;

namespace CivicSense.Facade.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _users;

        public AccountService(IUserRepository users)
        {
            _users = users;
        }

        public async Task<AuthenticatedUserDto> RegisterAsync(
            RegisterDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            FacadeValidation.Validate(dto);

            string fullName = dto.FullName.Trim();
            string email = NormalizeEmail(dto.Email);

            if (fullName.Length < 2)
            {
                throw new ValidationException(
                    "Full name must contain at least 2 characters.");
            }

            if (dto.Password.Length < 12 ||
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

            if (email.Length > 256)
            {
                throw new ValidationException(
                    "Email address is too long.");
            }

            if (await _users.EmailExistsAsync(email))
            {
                throw new ValidationException(
                    "An account with this email already exists.");
            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash =
                    PasswordHelper.HashPassword(dto.Password),
                Role = UserRole.Resident,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _users.AddAsync(user);

            return Map(user);
        }

        public async Task<AuthenticatedUserDto?> LoginAsync(
            LoginDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            FacadeValidation.Validate(dto);

            string email = NormalizeEmail(dto.Email);

            var user = await _users.GetByEmailAsync(email);

            if (user == null || !user.IsActive)
            {
                return null;
            }

            bool validPassword =
                PasswordHelper.VerifyPassword(
                    dto.Password,
                    user.PasswordHash);

            if (!validPassword)
            {
                return null;
            }

            return Map(user);
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        private static AuthenticatedUserDto Map(User user)
        {
            return new AuthenticatedUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}
