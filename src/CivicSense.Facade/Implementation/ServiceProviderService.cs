
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;

namespace CivicSense.Facade.Implementation
{
    public class ServiceProviderService
        : IServiceProviderService
    {
        private readonly IServiceProviderRepository _providers;
        private readonly IUserRepository _users;

        public ServiceProviderService(
            IServiceProviderRepository providers,
            IUserRepository users)
        {
            _providers = providers;
            _users = users;
        }

        public async Task<ServiceProviderDto> RegisterAsync(
            int actorId,
            ServiceProviderCreateDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            FacadeValidation.Validate(dto);

            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            if (user.Role != UserRole.Resident)
            {
                throw new UnauthorizedAccessException(
                    "Only residents can register providers.");
            }

            if (string.IsNullOrWhiteSpace(dto.BusinessName) ||
                string.IsNullOrWhiteSpace(dto.ServiceType) ||
                string.IsNullOrWhiteSpace(dto.ContactNumber) ||
                string.IsNullOrWhiteSpace(dto.ServiceArea))
            {
                throw new ValidationException(
                    "All provider details are required.");
            }

            var existing = await _providers
                .GetByUserIdAsync(actorId);

            if (existing != null)
            {
                throw new ValidationException(
                    "A provider profile already exists.");
            }

            var provider = new ServiceProvider
            {
                UserId = user.Id,
                BusinessName = dto.BusinessName.Trim(),
                ServiceType = dto.ServiceType.Trim(),
                ContactNumber = dto.ContactNumber.Trim(),
                ServiceArea = dto.ServiceArea.Trim(),
                IsVerified = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _providers.AddAsync(provider);

            return Map(provider);
        }

        public async Task VerifyAsync(
            int providerId, int adminId)
        {
            await FacadeValidation.RequireAdminAsync(
                _users, adminId);

            var provider = await _providers
                .GetByIdAsync(providerId);

            if (provider == null || !provider.IsActive)
            {
                throw new ValidationException(
                    "Active provider not found.");
            }

            if (provider.IsVerified)
                return;

            provider.IsVerified = true;

            await _providers.UpdateAsync(provider);
        }

        public async Task<List<ServiceProviderDto>>
            GetVerifiedAsync()
        {
            var providers = await _providers
                .GetVerifiedAsync();

            return providers.Select(Map).ToList();
        }

        private static ServiceProviderDto Map(
            ServiceProvider provider)
        {
            return new ServiceProviderDto
            {
                Id = provider.Id,
                BusinessName = provider.BusinessName,
                ServiceType = provider.ServiceType,
                ServiceArea = provider.ServiceArea,
                IsVerified = provider.IsVerified
            };
        }
    }
}
