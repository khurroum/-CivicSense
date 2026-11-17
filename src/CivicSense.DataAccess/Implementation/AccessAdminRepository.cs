using System.Data;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CivicSense.DataAccess.Implementation
{
    public class AccessAdminRepository
        : IAccessAdminRepository
    {
        private readonly ApplicationDbContext _context;

        public AccessAdminRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<User>> GetUsersAsync()
        {
            // Only retrieve fields needed for administration.
            return _context.Users
                .AsNoTracking()
                .OrderBy(x => x.FullName)
                .Select(x => new User
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Email = x.Email,
                    Role = x.Role,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        public Task<User?> GetUserAsync(
            int id, bool tracking = false)
        {
            IQueryable<User> query = _context.Users;

            if (!tracking)
                query = query.AsNoTracking();

            return query.SingleOrDefaultAsync(
                x => x.Id == id);
        }

        public Task<bool> IsActiveAdminAsync(int userId)
        {
            return _context.Users
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == userId &&
                    x.IsActive &&
                    x.Role == UserRoleLookup.Admin);
        }

        public Task<bool> EmailExistsAsync(
            string email, int excludedUserId)
        {
            return _context.Users.AnyAsync(x =>
                x.Id != excludedUserId &&
                x.Email == email);
        }

        public async Task<bool> HasOpenWorkAsync(int userId)
        {
            bool hasCivicWork = await _context.IssueAssignments
                .AnyAsync(x =>
                    x.StaffId == userId &&
                    x.IsActive &&
                    !x.Issue.IsArchived &&
                    x.Issue.Status != IssueStatusLookup.Resolved &&
                    x.Issue.Status != IssueStatusLookup.Rejected);

            if (hasCivicWork)
                return true;

            bool hasActiveProvider =
                await _context.ServiceProviders.AnyAsync(x =>
                    x.UserId == userId &&
                    x.IsActive);

            if (hasActiveProvider)
                return true;

            return await _context.ServiceRequests.AnyAsync(x =>
                (x.UserId == userId ||
                 x.ServiceProvider.UserId == userId) &&
                (
                    x.Status == ServiceRequestStatusLookup.Pending ||
                    x.Status == ServiceRequestStatusLookup.Accepted ||
                    x.Status == ServiceRequestStatusLookup.InProgress
                ));
        }

        public Task<List<Permission>> GetPermissionsAsync()
        {
            return _context.Permissions
                .AsNoTracking()
                .OrderBy(x => x.DisplayText)
                .ToListAsync();
        }

        public Task<List<PermissionGroup>> GetGroupsAsync()
        {
            return _context.PermissionGroups
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public Task<PermissionGroup?> GetGroupAsync(
            int id, bool tracking = false)
        {
            IQueryable<PermissionGroup> query =
                _context.PermissionGroups;

            if (!tracking)
                query = query.AsNoTracking();

            return query.SingleOrDefaultAsync(
                x => x.Id == id);
        }

        public Task<bool> GroupNameExistsAsync(
            string name, int excludedGroupId)
        {
            return _context.PermissionGroups.AnyAsync(x =>
                x.Id != excludedGroupId &&
                x.Name == name);
        }

        public Task<List<PermissionGroupMap>> GetMapsAsync(
            int? groupId = null,
            bool tracking = false)
        {
            IQueryable<PermissionGroupMap> query =
                _context.PermissionGroupMaps;

            if (!tracking)
                query = query.AsNoTracking();

            if (groupId.HasValue)
            {
                query = query.Where(x =>
                    x.PermissionGroupId == groupId.Value);
            }

            return query.OrderBy(x => x.Id).ToListAsync();
        }

        public Task<List<UserPermission>> GetGrantsAsync(
            int userId, bool tracking = false)
        {
            IQueryable<UserPermission> query =
                _context.UserPermissions;

            if (!tracking)
                query = query.AsNoTracking();

            return query
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Set<T>().Add(entity);
        }

        public Task<int> SaveAsync()
        {
            return _context.SaveChangesAsync();
        }

        public Task<IDbContextTransaction> BeginWriteAsync()
        {
            return _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);
        }
    }
}