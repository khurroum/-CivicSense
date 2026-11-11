
using CivicSense.DataAccess.Interface;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;
using CivicSense.Entities;

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

        public async Task<List<PaymentTransactionDto>>
            GetByRequestAsync(
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

            bool authorized =
                request.UserId == user.Id ||
                provider?.UserId == user.Id ||
                user.Role == UserRole.Admin;

            if (!authorized)
            {
                throw new UnauthorizedAccessException(
                    "You cannot view these payments.");
            }

            var payments = await _payments
                .GetByRequestAsync(serviceRequestId);

            return payments.Select(Map).ToList();
        }

        private static PaymentTransactionDto Map(
            PaymentTransaction payment)
        {
            return new PaymentTransactionDto
            {
                Id = payment.Id,
                ServiceRequestId =
                    payment.ServiceRequestId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt
            };
        }
    }
}
