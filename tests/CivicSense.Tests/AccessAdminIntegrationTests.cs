using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess;
using CivicSense.DataAccess.Implementation;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Implementation;
using CivicSense.Framework.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CivicSense.Tests
{
    public class AccessAdminIntegrationTests
    {
        [Fact]
        public async Task AccessAdministration_ShouldBeSecure()
        {
            var connection =
                @"Server=(localdb)\MSSQLLocalDB;" +
                $"Database=CivicSense_AccessAdmin_{Guid.NewGuid():N};" +
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

                var admin = new User
                {
                    FullName = "Test Administrator",
                    Email = "admin-access@example.test",
                    PasswordHash = "TEST_ONLY",
                    Role = UserRoleLookup.Admin,
                    IsActive = true
                };

                db.Users.Add(admin);

                await db.SaveChangesAsync();

                var permissionService =
                    new PermissionService(
                        new PermissionRepository(db));

                var accessService =
                    new AccessAdminService(
                        new AccessAdminRepository(db),
                        permissionService);

                var manageCategories =
                    await db.Permissions.SingleAsync(x =>
                        x.Name ==
                        PermissionCodes.ManageCategories);

                var manageAccess =
                    await db.Permissions.SingleAsync(x =>
                        x.Name ==
                        PermissionCodes.ManageAccess);

                // Default Admin has management access.
                Assert.True(
                    await permissionService.HasPermissionAsync(
                        admin.Id,
                        PermissionCodes.ManageAccess));

                // Create a group.
                int groupId =
                    await accessService.SaveGroupAsync(
                        0,
                        new GroupAdminInputDto
                        {
                            Name = "Integration Category Managers",
                            IsActive = true,
                            PermissionIds =
                                new[] { manageCategories.Id }
                        },
                        admin.Id);

                const string initialPassword =
                    "TestPassword!2026";

                // Create a Staff account with custom group.
                int staffId =
                    await accessService.SaveUserAsync(
                        0,
                        new UserAdminInputDto
                        {
                            FullName = "Test Staff Member",
                            Email = "staff-access@example.test",
                            Password = initialPassword,
                            ConfirmPassword = initialPassword,
                            Role = UserRoleLookup.Staff,
                            IsActive = true,
                            GroupIds = new[] { groupId }
                        },
                        admin.Id);

                var staff = await db.Users
                    .AsNoTracking()
                    .SingleAsync(x => x.Id == staffId);

                Assert.Equal(
                    UserRoleLookup.Staff,
                    staff.Role);

                // Password is stored securely.
                Assert.NotEqual(
                    initialPassword,
                    staff.PasswordHash);

                Assert.True(
                    PasswordHelper.VerifyPassword(
                        initialPassword,
                        staff.PasswordHash));

                // Group grants category permission.
                Assert.True(
                    await permissionService.HasPermissionAsync(
                        staffId,
                        PermissionCodes.ManageCategories));

                // Staff cannot administer users.
                await Assert.ThrowsAsync<
                    UnauthorizedAccessException>(
                    () => accessService.GetUsersAsync(staffId));

                // Direct denial overrides group membership.
                var userPage =
                    await accessService.GetUserPageAsync(
                        staffId, admin.Id);

                userPage.Form.DenyIds =
                    new[] { manageCategories.Id };

                await accessService.SaveUserAsync(
                    staffId,
                    userPage.Form,
                    admin.Id);

                Assert.False(
                    await permissionService.HasPermissionAsync(
                        staffId,
                        PermissionCodes.ManageCategories));

                // Check displayed effective permissions.
                userPage =
                    await accessService.GetUserPageAsync(
                        staffId, admin.Id);

                var categoryAccess = userPage.Permissions
                    .Single(x =>
                        x.Code ==
                        PermissionCodes.ManageCategories);

                Assert.Equal(
                    "Deny",
                    categoryAccess.DirectState);

                Assert.False(
                    categoryAccess.EffectiveAllowed);

                // Remove direct denial: inherit again.
                userPage.Form.DenyIds = Array.Empty<int>();

                await accessService.SaveUserAsync(
                    staffId,
                    userPage.Form,
                    admin.Id);

                Assert.True(
                    await permissionService.HasPermissionAsync(
                        staffId,
                        PermissionCodes.ManageCategories));

                // Protected permission cannot be granted
                // through a custom group.
                await Assert.ThrowsAsync<ValidationException>(
                    () => accessService.SaveGroupAsync(
                        0,
                        new GroupAdminInputDto
                        {
                            Name = "Invalid Management Group",
                            PermissionIds =
                                new[] { manageAccess.Id }
                        },
                        admin.Id));

                // Protected permission cannot be assigned
                // directly through this interface either.
                userPage =
                    await accessService.GetUserPageAsync(
                        staffId, admin.Id);

                userPage.Form.AllowIds =
                    new[] { manageAccess.Id };

                await Assert.ThrowsAsync<ValidationException>(
                    () => accessService.SaveUserAsync(
                        staffId,
                        userPage.Form,
                        admin.Id));

                // Stale user forms cannot overwrite changes.
                var oldUserPage =
                    await accessService.GetUserPageAsync(
                        staffId, admin.Id);

                var newUserPage =
                    await accessService.GetUserPageAsync(
                        staffId, admin.Id);

                newUserPage.Form.FullName =
                    "Updated Staff Member";

                await accessService.SaveUserAsync(
                    staffId,
                    newUserPage.Form,
                    admin.Id);

                oldUserPage.Form.GroupIds =
                    Array.Empty<int>();

                await Assert.ThrowsAsync<
                    DbUpdateConcurrencyException>(
                    () => accessService.SaveUserAsync(
                        staffId,
                        oldUserPage.Form,
                        admin.Id));

                // Stale group forms cannot overwrite changes.
                var oldGroupPage =
                    await accessService.GetGroupPageAsync(
                        groupId, admin.Id);

                var newGroupPage =
                    await accessService.GetGroupPageAsync(
                        groupId, admin.Id);

                newGroupPage.Form.Tag =
                    "Updated group description";

                await accessService.SaveGroupAsync(
                    groupId,
                    newGroupPage.Form,
                    admin.Id);

                await Assert.ThrowsAsync<
                    DbUpdateConcurrencyException>(
                    () => accessService.SaveGroupAsync(
                        groupId,
                        oldGroupPage.Form,
                        admin.Id));

                // Existing base role is protected.
                userPage =
                    await accessService.GetUserPageAsync(
                        staffId, admin.Id);

                userPage.Form.Role = UserRoleLookup.Admin;

                await Assert.ThrowsAsync<ValidationException>(
                    () => accessService.SaveUserAsync(
                        staffId, userPage.Form, admin.Id));

                // Cannot deactivate the protected Admin group.
                var defaultAdminGroup =
                    await db.PermissionGroups
                        .AsNoTracking()
                        .SingleAsync(x =>
                            x.DefaultRoleId ==
                            UserRoleLookup.Admin);

                var adminGroupPage =
                    await accessService.GetGroupPageAsync(
                        defaultAdminGroup.Id,
                        admin.Id);

                adminGroupPage.Form.IsActive = false;

                adminGroupPage.Form.PermissionIds =
                    adminGroupPage.Form.PermissionIds
                        .Where(x => x != manageAccess.Id)
                        .ToArray();

                await Assert.ThrowsAsync<ValidationException>(
                    () => accessService.SaveGroupAsync(
                        defaultAdminGroup.Id,
                        adminGroupPage.Form,
                        admin.Id));

                // Admin cannot deactivate their own account.
                var selfPage =
                    await accessService.GetUserPageAsync(
                        admin.Id, admin.Id);

                Assert.False(selfPage.CanEdit);

                selfPage.Form.IsActive = false;

                await Assert.ThrowsAsync<ValidationException>(
                    () => accessService.SaveUserAsync(
                        admin.Id,
                        selfPage.Form,
                        admin.Id));

                // Deactivate Staff with no outstanding work.
                userPage =
                    await accessService.GetUserPageAsync(
                        staffId, admin.Id);

                userPage.Form.IsActive = false;

                await accessService.SaveUserAsync(
                    staffId,
                    userPage.Form,
                    admin.Id);

                Assert.False(
                    await permissionService.HasPermissionAsync(
                        staffId,
                        PermissionCodes.ManageCategories));

                // Reactivate Staff account.
                userPage =
                    await accessService.GetUserPageAsync(
                        staffId, admin.Id);

                userPage.Form.IsActive = true;

                await accessService.SaveUserAsync(
                    staffId,
                    userPage.Form,
                    admin.Id);

                Assert.True(
                    await permissionService.HasPermissionAsync(
                        staffId,
                        PermissionCodes.ManageCategories));

                // Disable the permission group.
                var groupPage =
                    await accessService.GetGroupPageAsync(
                        groupId, admin.Id);

                groupPage.Form.IsActive = false;

                await accessService.SaveGroupAsync(
                    groupId,
                    groupPage.Form,
                    admin.Id);

                Assert.False(
                    await permissionService.HasPermissionAsync(
                        staffId,
                        PermissionCodes.ManageCategories));

                Assert.True(
                    await permissionService.HasPermissionAsync(
                        admin.Id,
                        PermissionCodes.ManageAccess));
            }
            finally
            {
                db.ChangeTracker.Clear();

                await db.Database.EnsureDeletedAsync();
            }
        }
    }
}