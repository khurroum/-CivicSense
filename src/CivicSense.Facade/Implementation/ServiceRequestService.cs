
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
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

        public async Task<ServiceRequestDto> CreateAsync(
            int actorId,
            ServiceRequestCreateDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            FacadeValidation.Validate(dto);

            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            if (user.Role != UserRole.Resident)
            {
                throw new UnauthorizedAccessException(
                    "Only residents can request services.");
            }

            var provider = await _providers
                .GetByIdAsync(dto.ServiceProviderId);

            if (provider == null ||
                !provider.IsVerified ||
                !provider.IsActive)
            {
                throw new ValidationException(
                    "Verified active provider not found.");
            }

            if (provider.UserId == user.Id)
            {
                throw new ValidationException(
                    "You cannot request your own service.");
            }

            if (string.IsNullOrWhiteSpace(dto.Description) ||
                string.IsNullOrWhiteSpace(dto.ServiceAddress))
            {
                throw new ValidationException(
                    "Description and address are required.");
            }

            if (dto.IssueId.HasValue)
            {
                var issue = await _issues
                    .GetByIdAsync(dto.IssueId.Value);

                if (issue == null ||
                    issue.IsArchived ||
                    issue.ReporterId != user.Id ||
                    issue.IssueType != IssueType.PersonalPrivate)
                {
                    throw new ValidationException(
                        "You can link only your own private issue.");
                }
            }

            var request = new ServiceRequest
            {
                UserId = user.Id,
                ServiceProviderId = provider.Id,
                IssueId = dto.IssueId,
                Description = dto.Description.Trim(),
                ServiceAddress = dto.ServiceAddress.Trim(),
                Status = ServiceRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _requests.AddAsync(request);

            return Map(request);
        }

        public async Task UpdateStatusAsync(
            int requestId,
            int actorId,
            ServiceRequestStatus newStatus)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            if (!Enum.IsDefined(newStatus))
            {
                throw new ValidationException(
                    "Invalid service request status.");
            }

            var request = await _requests
                .GetByIdAsync(requestId);

            if (request == null)
            {
                throw new KeyNotFoundException(
                    "Service request not found.");
            }

            var provider = await _providers
                .GetByIdAsync(request.ServiceProviderId);

            if (provider == null)
            {
                throw new ValidationException(
                    "Service provider not found.");
            }

            bool isCustomer = request.UserId == user.Id;

            bool isProvider =
                provider.UserId == user.Id &&
                provider.IsActive &&
                provider.IsVerified;

            bool allowed = false;

            // Resident may cancel a pending request.
            if (isCustomer &&
                request.Status == ServiceRequestStatus.Pending &&
                newStatus == ServiceRequestStatus.Cancelled)
            {
                allowed = true;
            }

            // Provider may accept or reject a pending request.
            if (isProvider &&
                request.Status == ServiceRequestStatus.Pending &&
                (newStatus == ServiceRequestStatus.Accepted ||
                 newStatus == ServiceRequestStatus.Rejected))
            {
                allowed = true;
            }

            // Provider may start accepted work.
            if (isProvider &&
                request.Status == ServiceRequestStatus.Accepted &&
                newStatus == ServiceRequestStatus.InProgress)
            {
                allowed = true;
            }

            // Provider may complete in-progress work.
            if (isProvider &&
                request.Status == ServiceRequestStatus.InProgress &&
                newStatus == ServiceRequestStatus.Completed)
            {
                allowed = true;
            }

            if (!allowed)
            {
                throw new ValidationException(
                    "This status transition is not permitted.");
            }

            request.Status = newStatus;
            request.UpdatedAt = DateTime.UtcNow;

            await _requests.UpdateAsync(request);
        }

        private static ServiceRequestDto Map(
            ServiceRequest request)
        {
            return new ServiceRequestDto
            {
                Id = request.Id,
                ServiceProviderId =
                    request.ServiceProviderId,
                IssueId = request.IssueId,
                Status = request.Status,
                EstimatedCost = request.EstimatedCost,
                CreatedAt = request.CreatedAt
            };
        }
    }
}
