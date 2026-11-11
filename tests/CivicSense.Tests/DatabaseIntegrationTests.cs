
using CivicSense.DataAccess;
using CivicSense.DataAccess.Implementation;
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Implementation;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CivicSense.Tests
{
    public class DatabaseIntegrationTests
    {
        private static string CreateTestConnectionString()
        {
            // Each test gets its own database.
            string databaseName =
                $"CivicSense_Integration_{Guid.NewGuid():N}";

            return
                @"Server=(localdb)\MSSQLLocalDB;" +
                $"Database={databaseName};" +
                "Trusted_Connection=True;" +
                "TrustServerCertificate=True;";
        }

        private static ApplicationDbContext CreateContext(
            string connectionString)
        {
            var options =
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

            return new ApplicationDbContext(options);
        }

        private static async Task<(int userId, int categoryId)>
            SeedBaseDataAsync(ApplicationDbContext context)
        {
            var user = new User
            {
                FullName = "Integration Test Resident",
                Email = "resident@example.test",

                // Test-only placeholder.
                // These tests do not perform authentication.
                PasswordHash = "TEST_ONLY_PLACEHOLDER",

                Role = UserRole.Resident,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var category = new Category
            {
                Name = "Test Roads",
                Description = "Integration test category",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            return (user.Id, category.Id);
        }

        [Fact]
        public async Task ConcurrentIssueUpdates_ShouldRejectStaleUpdate()
        {
            string connectionString =
                CreateTestConnectionString();

            try
            {
                int issueId;

                await using (var setup = CreateContext(
                    connectionString))
                {
                    await setup.Database.MigrateAsync();

                    var (userId, categoryId) =
                        await SeedBaseDataAsync(setup);

                    var issue = new Issue
                    {
                        ReporterId = userId,
                        CategoryId = categoryId,
                        Title = "Road surface damage",
                        Description =
                            "The road surface has been damaged.",
                        Address = "Test Road",
                        IssueType = IssueType.PublicCivic,
                        Status = IssueStatus.Reported,
                        Priority = IssuePriority.Medium,
                        CreatedAt = DateTime.UtcNow
                    };

                    setup.Issues.Add(issue);

                    await setup.SaveChangesAsync();

                    issueId = issue.Id;
                }

                await using var first =
                    CreateContext(connectionString);

                await using var second =
                    CreateContext(connectionString);

                var issueA = await first.Issues
                    .SingleAsync(x => x.Id == issueId);

                var issueB = await second.Issues
                    .SingleAsync(x => x.Id == issueId);

                // First request updates the record.
                issueA.Status = IssueStatus.UnderReview;

                await first.SaveChangesAsync();

                // Second request tries to update a stale version.
                issueB.Status = IssueStatus.Rejected;

                await Assert.ThrowsAsync<
                    DbUpdateConcurrencyException>(
                    async () =>
                    {
                        await second.SaveChangesAsync();
                    });

                await using var verification =
                    CreateContext(connectionString);

                var storedIssue = await verification.Issues
                    .AsNoTracking()
                    .SingleAsync(x => x.Id == issueId);

                Assert.Equal(
                    IssueStatus.UnderReview,
                    storedIssue.Status);
            }
            finally
            {
                await using var cleanup =
                    CreateContext(connectionString);

                await cleanup.Database.EnsureDeletedAsync();
            }
        }

        [Fact]
        public async Task CreateIssue_ShouldSaveInitialStatusHistory()
        {
            string connectionString =
                CreateTestConnectionString();

            try
            {
                await using var context =
                    CreateContext(connectionString);

                await context.Database.MigrateAsync();

                var (userId, categoryId) =
                    await SeedBaseDataAsync(context);

                var service = new IssueService(
                    new IssueRepository(context),
                    new UserRepository(context),
                    new CategoryRepository(context),
                    new IssueAssignmentRepository(context),
                    new IssueStatusHistoryRepository(context),
                    context);

                var dto = new IssueCreateDto
                {
                    Title = "Broken streetlight",
                    Description =
                        "The streetlight is not working at night.",
                    Address = "Test Street",
                    CategoryId = categoryId,
                    IssueType = IssueType.PublicCivic
                };

                var result = await service.CreateAsync(
                    dto, userId);

                await using var verification =
                    CreateContext(connectionString);

                var issue = await verification.Issues
                    .AsNoTracking()
                    .SingleAsync(x => x.Id == result.Id);

                var histories =
                    await verification.IssueStatusHistories
                        .AsNoTracking()
                        .Where(x => x.IssueId == issue.Id)
                        .ToListAsync();

                Assert.Equal(
                    IssueStatus.Reported,
                    issue.Status);

                Assert.Single(histories);

                Assert.Null(histories[0].OldStatus);

                Assert.Equal(
                    IssueStatus.Reported,
                    histories[0].NewStatus);
            }
            finally
            {
                await using var cleanup =
                    CreateContext(connectionString);

                await cleanup.Database.EnsureDeletedAsync();
            }
        }

        [Fact]
        public async Task FailedHistoryWrite_ShouldRollbackNewIssue()
        {
            string connectionString =
                CreateTestConnectionString();

            try
            {
                int userId;
                int categoryId;

                await using (var setup =
                    CreateContext(connectionString))
                {
                    await setup.Database.MigrateAsync();

                    (userId, categoryId) =
                        await SeedBaseDataAsync(setup);
                }

                await using (var context =
                    CreateContext(connectionString))
                {
                    var service = new IssueService(
                        new IssueRepository(context),
                        new UserRepository(context),
                        new CategoryRepository(context),
                        new IssueAssignmentRepository(context),
                        new FailingHistoryRepository(),
                        context);

                    var dto = new IssueCreateDto
                    {
                        Title = "Rollback test issue",
                        Description =
                            "This operation must be rolled back.",
                        Address = "Rollback Street",
                        CategoryId = categoryId,
                        IssueType = IssueType.PublicCivic
                    };

                    await Assert.ThrowsAsync<
                        InvalidOperationException>(
                        async () =>
                        {
                            await service.CreateAsync(
                                dto, userId);
                        });
                }

                await using var verification =
                    CreateContext(connectionString);

                bool exists = await verification.Issues
                    .AnyAsync(x =>
                        x.Title == "Rollback test issue");

                Assert.False(exists);
            }
            finally
            {
                await using var cleanup =
                    CreateContext(connectionString);

                await cleanup.Database.EnsureDeletedAsync();
            }
        }

        private sealed class FailingHistoryRepository
            : IIssueStatusHistoryRepository
        {
            public Task<List<IssueStatusHistory>>
                GetAllAsync()
            {
                return Task.FromResult(
                    new List<IssueStatusHistory>());
            }

            public Task<IssueStatusHistory?>
                GetByIdAsync(int id)
            {
                return Task.FromResult<IssueStatusHistory?>(
                    null);
            }

            public Task AddAsync(
                IssueStatusHistory entity)
            {
                throw new InvalidOperationException(
                    "Simulated status-history failure.");
            }

            public Task UpdateAsync(
                IssueStatusHistory entity)
            {
                throw new InvalidOperationException(
                    "Not supported in this test.");
            }

            public Task<bool> DeleteAsync(int id)
            {
                throw new InvalidOperationException(
                    "Not supported in this test.");
            }
        }
    }
}
