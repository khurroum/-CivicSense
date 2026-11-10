
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;

namespace CivicSense.Facade.Implementation
{
    public class IssueService : IIssueService
    {
        private readonly IIssueRepository _issues;
        private readonly IUserRepository _users;
        private readonly ICategoryRepository _categories;
        private readonly IIssueAssignmentRepository _assignments;

        public IssueService(
            IIssueRepository issues,
            IUserRepository users,
            ICategoryRepository categories,
            IIssueAssignmentRepository assignments)
        {
            _issues = issues;
            _users = users;
            _categories = categories;
            _assignments = assignments;
        }

        public async Task<IssueDto> CreateAsync(
            IssueCreateDto dto, int actorId)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            if (user.Role != UserRole.Resident)
            {
                throw new UnauthorizedAccessException(
                    "Only residents can submit issues.");
            }

            FacadeValidation.Validate(dto);

            ValidateIssueInput(
                dto.Title,
                dto.Description,
                dto.Address,
                dto.Latitude,
                dto.Longitude);

            if (!Enum.IsDefined(dto.IssueType))
            {
                throw new ValidationException(
                    "Invalid issue type.");
            }

            await ValidateCategoryAsync(dto.CategoryId);

            var issue = new Issue
            {
                ReporterId = user.Id,
                CategoryId = dto.CategoryId,
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                Address = dto.Address.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                IssueType = dto.IssueType,
                Status = IssueStatus.Reported,
                Priority = IssuePriority.Medium,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            await _issues.AddAsync(issue);

            return Map(issue);
        }

        public async Task<List<IssueDto>> GetMyIssuesAsync(
            int actorId)
        {
            await FacadeValidation.GetActiveUserAsync(
                _users, actorId);

            var issues = await _issues
                .GetByReporterAsync(actorId);

            return issues.Select(Map).ToList();
        }

        public async Task<IssueDto?> GetByIdAsync(
            int issueId, int actorId)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            var issue = await _issues.GetByIdAsync(issueId);

            if (issue == null || issue.IsArchived)
                return null;

            // The resident who submitted it can view it.
            if (issue.ReporterId == user.Id)
                return Map(issue);

            // Admin can review public civic reports.
            if (user.Role == UserRole.Admin &&
                issue.IssueType == IssueType.PublicCivic)
            {
                return Map(issue);
            }

            // Staff may view civic issues assigned to them.
            if (user.Role == UserRole.Staff &&
                issue.IssueType == IssueType.PublicCivic)
            {
                var assignment = await _assignments
                    .GetActiveByIssueAsync(issueId);

                if (assignment?.StaffId == user.Id)
                    return Map(issue);
            }

            // Other access is denied, including private reports.
            throw new UnauthorizedAccessException(
                "You cannot access this issue.");
        }

        public async Task<bool> UpdateAsync(
            int issueId,
            IssueUpdateDto dto,
            int actorId)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            FacadeValidation.Validate(dto);

            ValidateIssueInput(
                dto.Title,
                dto.Description,
                dto.Address,
                dto.Latitude,
                dto.Longitude);

            var issue = await _issues.GetByIdAsync(issueId);

            if (issue == null || issue.IsArchived)
                return false;

            if (issue.ReporterId != user.Id)
            {
                throw new UnauthorizedAccessException(
                    "Only the reporter can edit this issue.");
            }

            if (issue.Status != IssueStatus.Reported &&
                issue.Status != IssueStatus.NeedsInformation)
            {
                throw new ValidationException(
                    "This issue cannot be edited in its current status.");
            }

            await ValidateCategoryAsync(dto.CategoryId);

            issue.Title = dto.Title.Trim();
            issue.Description = dto.Description.Trim();
            issue.Address = dto.Address.Trim();
            issue.CategoryId = dto.CategoryId;
            issue.Latitude = dto.Latitude;
            issue.Longitude = dto.Longitude;
            issue.UpdatedAt = DateTime.UtcNow;

            await _issues.UpdateAsync(issue);

            return true;
        }

        public async Task<bool> ArchiveAsync(
            int issueId, int actorId)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            var issue = await _issues.GetByIdAsync(issueId);

            if (issue == null || issue.IsArchived)
                return false;

            if (issue.ReporterId != user.Id)
            {
                throw new UnauthorizedAccessException(
                    "Only the reporter can archive this issue.");
            }

            // Active civic work must not disappear from
            // administrative or staff queues.
            if (issue.Status != IssueStatus.Resolved &&
                issue.Status != IssueStatus.Rejected)
            {
                throw new ValidationException(
                    "Only resolved or rejected reports can be archived.");
            }

            issue.IsArchived = true;
            issue.UpdatedAt = DateTime.UtcNow;

            await _issues.UpdateAsync(issue);

            return true;
        }

        private async Task ValidateCategoryAsync(int categoryId)
        {
            var category = await _categories
                .GetByIdAsync(categoryId);

            if (category == null || !category.IsActive)
            {
                throw new ValidationException(
                    "Please select an active issue category.");
            }
        }

        private static void ValidateIssueInput(
            string title,
            string description,
            string address,
            decimal? latitude,
            decimal? longitude)
        {
            if (string.IsNullOrWhiteSpace(title) ||
                title.Trim().Length < 5)
            {
                throw new ValidationException(
                    "Issue title must have at least 5 characters.");
            }

            if (string.IsNullOrWhiteSpace(description) ||
                description.Trim().Length < 10)
            {
                throw new ValidationException(
                    "Description must have at least 10 characters.");
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ValidationException(
                    "Address is required.");
            }

            if (latitude.HasValue != longitude.HasValue)
            {
                throw new ValidationException(
                    "Latitude and longitude must both be supplied.");
            }
        }

        private static IssueDto Map(Issue issue)
        {
            return new IssueDto
            {
                Id = issue.Id,
                ReporterId = issue.ReporterId,
                CategoryId = issue.CategoryId,
                Title = issue.Title,
                Description = issue.Description,
                Address = issue.Address,
                Latitude = issue.Latitude,
                Longitude = issue.Longitude,
                IssueType = issue.IssueType,
                Status = issue.Status,
                Priority = issue.Priority,
                CreatedAt = issue.CreatedAt
            };
        }
    }
}
