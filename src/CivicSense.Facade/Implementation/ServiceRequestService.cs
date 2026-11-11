
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.Interface;

namespace CivicSense.Facade.Implementation
{
    public class ServiceRequestService
        : IServiceRequestService
    {
        private readonly IServiceRequestRepository _requests;
        private readonly IServiceProviderRepository _providers;
        private readonly IIssueRepository _issues;
        private readonly IUserRepository _users;

        public ServiceRequestService(
            IServiceRequestRepository requests,
            IServiceProviderRepository providers,
            IIssueRepository issues,
            IUserRepository users)
        {
            _requests = requests;
            _providers = providers;
            _issues = issues;
            _users = users;
        }

        public async Task<ServiceRequest> CreateAsync(
            int actorId, int providerId,
            int? issueId, string description,
            string serviceAddress)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            if (user.Role != UserRole.Resident)
                throw new UnauthorizedAccessException();

            var provider = await _providers
                .GetByIdAsync(providerId);

            if (provider == null ||
                !provider.IsActive ||
                !provider.IsVerified)
                throw new ValidationException(
                    "Verified service provider not found.");

            if (provider.UserId == actorId)
                throw new ValidationException(
                    "You cannot request your own service.");

            if (string.IsNullOrWhiteSpace(description) ||
                description.Trim().Length > 2000 ||
                string.IsNullOrWhiteSpace(serviceAddress) ||
                serviceAddress.Trim().Length > 500)
                throw new ValidationException(
                    "Valid service description and address required.");

            if (issueId.HasValue)
            {
                var issue = await _issues
                    .GetByIdAsync(issueId.Value);

                if (issue == null ||
                    issue.IsArchived ||
                    issue.ReporterId != actorId ||
                    issue.IssueType != IssueType.PersonalPrivate)
                    throw new ValidationException(
                        "Only your own private issue can be linked.");
            }

            var request = new ServiceRequest
            {
                UserId = actorId,
                ServiceProviderId = providerId,
                IssueId = issueId,
                Description = description.Trim(),
                ServiceAddress = serviceAddress.Trim(),
                Status = ServiceRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _requests.AddAsync(request);

            return request;
        }

        public async Task UpdateStatusAsync(
            int requestId, int actorId,
            ServiceRequestStatus newStatus)
        {
            await FacadeValidation.GetActiveUserAsync(
                _users, actorId);

            if (!Enum.IsDefined(newStatus))
                throw new ValidationException(
                    "Invalid service request status.");

            var request = await _requests.GetByIdAsync(
                requestId);

            if (request == null)
                throw new KeyNotFoundException(
                    "Service request not found.");

            var provider = await _providers
                .GetByIdAsync(request.ServiceProviderId);

            if (provider == null)
                throw new ValidationException(
                    "Provider not found.");

            bool isCustomer = request.UserId == actorId;

            bool isProvider =
                provider.UserId == actorId &&
                provider.IsVerified &&
                provider.IsActive;

            bool allowed =
                (isCustomer &&
                 request.Status == ServiceRequestStatus.Pending &&
                 newStatus == ServiceRequestStatus.Cancelled)
                ||
                (isProvider &&
                 request.Status == ServiceRequestStatus.Pending &&
                 (newStatus == ServiceRequestStatus.Accepted ||
                  newStatus == ServiceRequestStatus.Rejected))
                ||
                (isProvider &&
                 request.Status == ServiceRequestStatus.Accepted &&
                 newStatus == ServiceRequestStatus.InProgress)
                ||
                (isProvider &&
                 request.Status == ServiceRequestStatus.InProgress &&
                 newStatus == ServiceRequestStatus.Completed);

            if (!allowed)
                throw new UnauthorizedAccessException(
                    "This status transition is not permitted.");

            request.Status = newStatus;
            request.UpdatedAt = DateTime.UtcNow;

            await _requests.UpdateAsync(request);
        }
    }
}
