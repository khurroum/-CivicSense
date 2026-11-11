
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.Interface;

namespace CivicSense.Facade.Implementation
{
    public class ServiceProviderService : IServiceProviderService
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

        public async Task<ServiceProvider> RegisterAsync(
            int actorId,
            string businessName,
            string serviceType,
            string contactNumber,
            string serviceArea)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            if (user.Role != UserRole.Resident)
            {
                throw new UnauthorizedAccessException(
                    "Only resident accounts can register providers.");
            }

            if (string.IsNullOrWhiteSpace(businessName) ||
                businessName.Trim().Length > 150 ||
                string.IsNullOrWhiteSpace(serviceType) ||
                serviceType.Trim().Length > 100 ||
                string.IsNullOrWhiteSpace(contactNumber) ||
                contactNumber.Trim().Length > 20 ||
                string.IsNullOrWhiteSpace(serviceArea) ||
                serviceArea.Trim().Length > 200)
            {
                throw new ValidationException(
                    "Invalid service provider information.");
            }

            // Retrieve provider profile for this user only.
            var existing = await _providers
                .GetByUserIdAsync(actorId);

            if (existing != null)
            {
                throw new ValidationException(
                    "A provider profile already exists.");
            }

            var provider = new ServiceProvider
            {
                UserId = actorId,
                BusinessName = businessName.Trim(),
                ServiceType = serviceType.Trim(),
                ContactNumber = contactNumber.Trim(),
                ServiceArea = serviceArea.Trim(),
                IsVerified = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _providers.AddAsync(provider);

            return provider;
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
                    "Active service provider not found.");
            }

            provider.IsVerified = true;

            await _providers.UpdateAsync(provider);
        }

        public async Task<List<ServiceProvider>> GetVerifiedAsync()
        {
            return await _providers.GetVerifiedAsync();
        }
    }
}
