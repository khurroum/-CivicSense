
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CivicSense.DataAccess;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;
using Microsoft.EntityFrameworkCore;

namespace CivicSense.Facade.Implementation
{
    public class IssueService : IIssueService
    {
        private readonly IIssueRepository _issues;
        private readonly IUserRepository _users;
        private readonly ICategoryRepository _categories;
        private readonly IIssueAssignmentRepository _assignments;
        private readonly IIssueStatusHistoryRepository _history;
        private readonly ApplicationDbContext _context;

        public IssueService(
            IIssueRepository issues,
            IUserRepository users,
            ICategoryRepository categories,
            IIssueAssignmentRepository assignments,
            IIssueStatusHistoryRepository history,
            ApplicationDbContext context)
        {
            _issues = issues;
            _users = users;
            _categories = categories;
            _assignments = assignments;
            _history = history;
            _context = context;
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

            // Issue and initial history are saved together.
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            await _issues.AddAsync(issue);

            var statusHistory = new IssueStatusHistory
            {
                IssueId = issue.Id,
                OldStatus = null,
                NewStatus = IssueStatus.Reported,
                ChangedById = user.Id,
                Note = "Issue reported by resident.",
                ChangedAt = DateTime.UtcNow
            };

            await _history.AddAsync(statusHistory);

            await transaction.CommitAsync();

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
            {
                return null;
            }

            // Reporter can view their own report.
            if (issue.ReporterId == user.Id)
            {
                return Map(issue);
            }

            // Admin can view public civic reports.
            if (user.Role == UserRole.Admin &&
                issue.IssueType == IssueType.PublicCivic)
            {
                return Map(issue);
            }

            // Assigned staff can view public civic issues.
            if (user.Role == UserRole.Staff &&
                issue.IssueType == IssueType.PublicCivic)
            {
                var assignment = await _assignments
                    .GetActiveByIssueAsync(issueId);

                if (assignment?.StaffId == user.Id)
                {
                    return Map(issue);
                }
            }

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

            if (user.Role != UserRole.Resident)
            {
                throw new UnauthorizedAccessException(
                    "Only residents can edit their issues.");
            }

            FacadeValidation.Validate(dto);

            ValidateIssueInput(
                dto.Title,
                dto.Description,
                dto.Address,
                dto.Latitude,
                dto.Longitude);

            // Validate the submitted concurrency token.
            if (dto.RowVersion == null ||
                dto.RowVersion.Length != 8)
            {
                throw new ValidationException(
                    "Invalid issue version. Please reload the issue.");
            }

            var issue = await _issues.GetByIdAsync(issueId);

            if (issue == null || issue.IsArchived)
            {
                return false;
            }

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

            // First concurrency check:
            // Compare the browser's original version with
            // the latest version retrieved from SQL Server.
            if (issue.RowVersion == null ||
                !issue.RowVersion.SequenceEqual(dto.RowVersion))
            {
                throw new DbUpdateConcurrencyException(
                    "This issue was modified by another user. " +
                    "Please reload the latest version.");
            }

            await ValidateCategoryAsync(dto.CategoryId);

            // Ensure the entity is tracked by this DbContext.
            // This also supports repositories using AsNoTracking.
            var entry = _context.Entry(issue);

            if (entry.State == EntityState.Detached)
            {
                _context.Issues.Attach(issue);
            }

            // Tell EF Core which original version the browser
            // submitted. SQL Server checks this during UPDATE.
            _context.Entry(issue)
                .Property(x => x.RowVersion)
                .OriginalValue = dto.RowVersion.ToArray();

            // Update only permitted properties.
            issue.Title = dto.Title.Trim();
            issue.Description = dto.Description.Trim();
            issue.Address = dto.Address.Trim();
            issue.CategoryId = dto.CategoryId;
            issue.Latitude = dto.Latitude;
            issue.Longitude = dto.Longitude;
            issue.UpdatedAt = DateTime.UtcNow;

            // Second concurrency check occurs during SaveChangesAsync
            // inside the repository implementation.
            await _issues.UpdateAsync(issue);

            return true;
        }

        public async Task<bool> ArchiveAsync(
            int issueId, int actorId)
        {
            var user = await FacadeValidation
                .GetActiveUserAsync(_users, actorId);

            if (user.Role != UserRole.Resident)
            {
                throw new UnauthorizedAccessException(
                    "Only residents can archive their issues.");
            }

            var issue = await _issues.GetByIdAsync(issueId);

            if (issue == null || issue.IsArchived)
            {
                return false;
            }

            if (issue.ReporterId != user.Id)
            {
                throw new UnauthorizedAccessException(
                    "Only the reporter can archive this issue.");
            }

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
                CreatedAt = issue.CreatedAt,

                // Return the current SQL Server concurrency token.
                RowVersion = issue.RowVersion?.ToArray()
                    ?? Array.Empty<byte>()
            };
        }
    }
}
