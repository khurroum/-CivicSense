
using CivicSense.DataAccess;
using CivicSense.DataAccess.Implementation;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Implementation;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CivicSense.Tests
{
    public class PermissionIntegrationTests
    {
        [Fact]
        public async Task Permissions_ShouldSupportGroupsDirectGrantsAndDenies()
        {
            var connection =
                @"Server=(localdb)\MSSQLLocalDB;" +
                $"Database=CivicSense_Permissions_{Guid.NewGuid():N};" +
                "Trusted_Connection=True;" +
                "TrustServerCertificate=True;";

            var options =
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(connection)
                    .Options;

            await using var db =
                new ApplicationDbContext(options);

            try
            {
                await db.Database.MigrateAsync();

                var staff = new User
                {
                    FullName = "Staff Test",
                    Email = "staff@example.test",
                    PasswordHash = "TEST_ONLY",
                    Role = UserRoleLookup.Staff
                };

                var admin = new User
                {
                    FullName = "Admin Test",
                    Email = "admin@example.test",
                    PasswordHash = "TEST_ONLY",
                    Role = UserRoleLookup.Admin
                };

                db.Users.AddRange(staff, admin);

                await db.SaveChangesAsync();

                var service = new PermissionService(
                    new PermissionRepository(db));

                var categoryService = new CategoryService(
                    new CategoryRepository(db),
                    service);

                var code = PermissionCodes.ManageCategories;

                var permission = await db.Permissions
                    .SingleAsync(x => x.Name == code);

                // Default Admin has permission.
                Assert.True(
                    await service.HasPermissionAsync(
                        admin.Id, code));

                // Staff initially has no permission.
                Assert.False(
                    await service.HasPermissionAsync(
                        staff.Id, code));

                Assert.False(
                    await service.HasPermissionAsync(
                        staff.Id, "Unknown.Permission"));

                var dto = new CategoryCreateDto
                {
                    Name = "Permission Test Roads"
                };

                await Assert.ThrowsAsync<
                    UnauthorizedAccessException>(
                    () => categoryService.CreateAsync(
                        dto, staff.Id));

                // Create a custom permission group.
                var group = new PermissionGroup
                {
                    Name = "Category Managers"
                };

                db.PermissionGroups.Add(group);

                await db.SaveChangesAsync();

                var map = new PermissionGroupMap
                {
                    PermissionGroupId = group.Id,
                    PermissionId = permission.Id
                };

                var membership = new UserPermission
                {
                    UserId = staff.Id,
                    PermissionGroupId = group.Id
                };

                db.AddRange(map, membership);

                await db.SaveChangesAsync();

                // Staff inherits group permission.
                Assert.True(
                    await service.HasPermissionAsync(
                        staff.Id, code));

                var category =
                    await categoryService.CreateAsync(
                        dto, staff.Id);

                Assert.True(
                    await categoryService.DeactivateAsync(
                        category.Id, staff.Id));

                Assert.True(
                    await categoryService.ReactivateAsync(
                        category.Id, staff.Id));

                // Individual deny overrides group grant.
                var direct = new UserPermission
                {
                    UserId = staff.Id,
                    PermissionId = permission.Id,
                    IsAllowed = false
                };

                db.UserPermissions.Add(direct);

                await db.SaveChangesAsync();

                Assert.False(
                    await service.HasPermissionAsync(
                        staff.Id, code));

                await Assert.ThrowsAsync<
                    UnauthorizedAccessException>(
                    () => categoryService.DeactivateAsync(
                        category.Id, staff.Id));

                // Direct grant works independently.
                direct.IsAllowed = true;

                group.IsActive = false;

                await db.SaveChangesAsync();

                Assert.True(
                    await service.HasPermissionAsync(
                        staff.Id, code));

                // Deactivating all grants removes access.
                direct.IsActive = false;

                await db.SaveChangesAsync();

                Assert.False(
                    await service.HasPermissionAsync(
                        staff.Id, code));

                // Reactivate group.
                group.IsActive = true;

                await db.SaveChangesAsync();

                Assert.True(
                    await service.HasPermissionAsync(
                        staff.Id, code));

                // Inactive user cannot use permissions.
                staff.IsActive = false;

                await db.SaveChangesAsync();

                Assert.False(
                    await service.HasPermissionAsync(
                        staff.Id, code));

                // Admin can also lose permissions.
                db.UserPermissions.Add(new UserPermission
                {
                    UserId = admin.Id,
                    PermissionId = permission.Id,
                    IsAllowed = false
                });

                await db.SaveChangesAsync();

                Assert.False(
                    await service.HasPermissionAsync(
                        admin.Id, code));

                await Assert.ThrowsAsync<
                    UnauthorizedAccessException>(
                    () => categoryService.GetAllAsync(
                        admin.Id));

                // Invalid assignment must fail.
                db.UserPermissions.Add(new UserPermission
                {
                    UserId = admin.Id
                });

                await Assert.ThrowsAsync<DbUpdateException>(
                    () => db.SaveChangesAsync());
            }
            finally
            {
                db.ChangeTracker.Clear();

                await db.Database.EnsureDeletedAsync();
            }
        }
    }
}
