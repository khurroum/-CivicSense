using CivicSense.DataAccess;
using CivicSense.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CivicSense.Tests
{
    public class LookupIntegrationTests
    {
        [Fact]
        public async Task LookupTables_ShouldContainCanonicalIdsAndRejectInvalidRole()
        {
            // This test uses a separate, temporary LocalDB database.
            var connectionString =
                @"Server=(localdb)\MSSQLLocalDB;" +
                $"Database=CivicSense_Lookups_{Guid.NewGuid():N};" +
                "Trusted_Connection=True;TrustServerCertificate=True;";

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString).Options;

            await using var context = new ApplicationDbContext(options);
            try
            {
                await context.Database.MigrateAsync();

                var roles = await context.UserRoles.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
                Assert.Equal(new byte[] { 1, 2, 3 }, roles.Select(x => x.Id).ToArray());
                Assert.Equal(new[] { "Resident", "Staff", "Admin" }, roles.Select(x => x.Name).ToArray());
                Assert.Equal(2, await context.IssueTypes.CountAsync());
                Assert.Equal(8, await context.IssueStatuses.CountAsync());
                Assert.Equal(4, await context.IssuePriorities.CountAsync());
                Assert.Equal(6, await context.ServiceRequestStatuses.CountAsync());
                Assert.Equal(6, await context.PaymentStatuses.CountAsync());

                var user = new User
                {
                    FullName = "Lookup Integration Resident",
                    Email = "lookup@example.test",
                    PasswordHash = "TEST_ONLY_PLACEHOLDER",
                    Role = UserRoleLookup.Resident,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                user.Role = 255; // No matching UserRoles record exists.
                var error = await Assert.ThrowsAsync<DbUpdateException>(
                    () => context.SaveChangesAsync());
                Assert.Equal(547, Assert.IsType<SqlException>(error.InnerException).Number);
            }
            finally
            {
                context.ChangeTracker.Clear();
                await context.Database.EnsureDeletedAsync();
            }
        }

        [Fact]
        public void LookupIds_ShouldKeepCanonicalNamesAndRejectUnknownIds()
        {
            Assert.Equal("Resident", UserRoleLookup.NameOf(UserRoleLookup.Resident));
            Assert.Equal("Staff", UserRoleLookup.NameOf(UserRoleLookup.Staff));
            Assert.Equal("Admin", UserRoleLookup.NameOf(UserRoleLookup.Admin));
            Assert.Equal("Resolved", IssueStatusLookup.NameOf(IssueStatusLookup.Resolved));
            Assert.False(UserRoleLookup.IsDefined(0));
            Assert.False(UserRoleLookup.IsDefined(255));
            Assert.False(IssueTypeLookup.IsDefined(255));
            Assert.False(ServiceRequestStatusLookup.IsDefined(255));
        }
    }
}
