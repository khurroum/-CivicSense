
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.Interface;

namespace CivicSense.Facade.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentTransactionRepository _payments;
        private readonly IServiceRequestRepository _requests;
        private readonly IServiceProviderRepository _providers;
        private readonly IUserRepository _users;

        public PaymentService(
            IPaymentTransactionRepository payments,
            IServiceRequestRepository requests,
            IServiceProviderRepository providers,
            IUserRepository users)
        {
            _payments = payments;
            _requests = requests;
            _providers = providers;
            _users = users;
        }

        public async Task<List<PaymentTransaction>> GetByRequestAsync(
            int serviceRequestId,
            int actorId)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            var request = await _requests
                .GetByIdAsync(serviceRequestId);

            if (request == null)
            {
                throw new KeyNotFoundException(
                    "Service request not found.");
            }

            var provider = await _providers
                .GetByIdAsync(request.ServiceProviderId);

            bool canView =
                request.UserId == actorId ||
                provider?.UserId == actorId ||
                user.Role == UserRole.Admin;

            if (!canView)
            {
                throw new UnauthorizedAccessException(
                    "You cannot view these payment records.");
            }

            // Only retrieve payments linked to this request.
            return await _payments
                .GetByRequestAsync(serviceRequestId);
        }
    }
}
