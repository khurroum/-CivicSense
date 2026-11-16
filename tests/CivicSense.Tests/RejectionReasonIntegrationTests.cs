using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess;
using CivicSense.DataAccess.Implementation;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Implementation;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CivicSense.Tests
{
    public class RejectionReasonIntegrationTests
    {
        [Fact]
        public async Task Rejection_ShouldExposeOnlyReasonToOwnerAndNotifyOwner()
        {
            // Never uses or deletes the development CivicSenseDB database.
            var connectionString =
                @"Server=(localdb)\MSSQLLocalDB;" +
                $"Database=CivicSense_Rejection_{Guid.NewGuid():N};" +
                "Trusted_Connection=True;TrustServerCertificate=True;";

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString).Options;

            await using var context = new ApplicationDbContext(options);
            try
            {
                await context.Database.MigrateAsync();

                var owner = NewUser("Owner", UserRoleLookup.Resident);
                var other = NewUser("Other", UserRoleLookup.Resident);
                var admin = NewUser("Admin", UserRoleLookup.Admin);
                var category = new Category
                {
                    Name = "Test Roads",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.AddRange(owner, other, admin);
                context.Categories.Add(category);
                await context.SaveChangesAsync();

                var issues = new IssueRepository(context);
                var users = new UserRepository(context);
                var assignments = new IssueAssignmentRepository(context);
                var history = new IssueStatusHistoryRepository(context);
                var notifications = new NotificationRepository(context);
                var issueService = new IssueService(issues, users,
                    new CategoryRepository(context), assignments, history, context);
                var workflow = new IssueWorkflowService(context, issues, users,
                    assignments, history, notifications,
                    new IssueWorkflowQueryRepository(context));
                var comments = new IssueCommentService(
                    new IssueCommentRepository(context), issues, users, assignments);
                var notificationService = new NotificationService(notifications, users);

                var created = await issueService.CreateAsync(new IssueCreateDto
                {
                    Title = "Test damaged road",
                    Description = "The road needs maintenance near the school.",
                    Address = "Test Street",
                    CategoryId = category.Id,
                    IssueType = IssueTypeLookup.PublicCivic
                }, owner.Id);

                await workflow.ReviewAsync(created.Id, admin.Id);
                await comments.AddAsync(created.Id, admin.Id, new CommentCreateDto
                {
                    Content = "CONFIDENTIAL_INTERNAL_NOTE",
                    IsInternal = true
                });

                await Assert.ThrowsAsync<ValidationException>(() =>
                    workflow.RejectAsync(created.Id, admin.Id, "   "));
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                    workflow.RejectAsync(created.Id, other.Id, "Not authorized"));
                Assert.Equal(IssueStatusLookup.UnderReview,
                    (await issueService.GetByIdAsync(created.Id, owner.Id))!.Status);

                // Exercise the maximum permitted explanation length.
                string reason = new string('R', 1000);
                await workflow.RejectAsync(created.Id, admin.Id, " " + reason + " ");
                context.ChangeTracker.Clear();

                var result = await issueService.GetByIdAsync(created.Id, owner.Id);
                Assert.NotNull(result);
                Assert.Equal(IssueStatusLookup.Rejected, result.Status);
                Assert.Equal(reason, result.RejectionReason);

                await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                    issueService.GetByIdAsync(created.Id, other.Id));
                Assert.Null(await issues.GetRejectionReasonForOwnerAsync(created.Id, other.Id));
                Assert.Null((await issueService.GetByIdAsync(created.Id, admin.Id))!.RejectionReason);

                Assert.Empty(await comments.GetByIssueAsync(created.Id, owner.Id));
                Assert.Single(await comments.GetByIssueAsync(created.Id, admin.Id));
                var ownerNotifications = await notificationService.GetMineAsync(owner.Id);
                var rejection = Assert.Single(ownerNotifications.Where(n =>
                    n.IssueId == created.Id && n.Title == "Issue review completed"));
                Assert.Equal("Your issue was rejected. Reason: " + reason, rejection.Message);
                Assert.Empty(await notificationService.GetMineAsync(other.Id));
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                    notificationService.MarkReadAsync(rejection.Id, other.Id));

                var saved = await context.IssueStatusHistories.AsNoTracking()
                    .SingleAsync(h => h.IssueId == created.Id &&
                        h.NewStatus == IssueStatusLookup.Rejected);
                Assert.Equal(reason, saved.Note);

                // Simulate a rejection made before Step 8.4.1.
                var oldIssue = new Issue
                {
                    ReporterId = owner.Id,
                    CategoryId = category.Id,
                    Title = "Previously rejected road issue",
                    Description = "This issue was rejected before the update.",
                    Address = "Old Test Street",
                    IssueType = IssueTypeLookup.PublicCivic,
                    Status = IssueStatusLookup.Rejected,
                    Priority = IssuePriorityLookup.Medium
                };
                await issues.AddAsync(oldIssue);
                var now = DateTime.UtcNow;
                await history.AddAsync(new IssueStatusHistory
                {
                    IssueId = oldIssue.Id,
                    ChangedById = admin.Id,
                    OldStatus = IssueStatusLookup.Reported,
                    NewStatus = IssueStatusLookup.UnderReview,
                    Note = "CONFIDENTIAL_REVIEW_NOTE",
                    ChangedAt = now.AddMinutes(-1)
                });
                await history.AddAsync(new IssueStatusHistory
                {
                    IssueId = oldIssue.Id,
                    ChangedById = admin.Id,
                    OldStatus = IssueStatusLookup.UnderReview,
                    NewStatus = IssueStatusLookup.Rejected,
                    Note = "Earlier rejection explanation",
                    ChangedAt = now
                });
                Assert.Equal("Earlier rejection explanation",
                    (await issueService.GetByIdAsync(oldIssue.Id, owner.Id))!.RejectionReason);

                // Archive access remains unchanged until Step 8.4.2.
                Assert.True(await issueService.ArchiveAsync(oldIssue.Id, owner.Id));
                Assert.Null(await issueService.GetByIdAsync(oldIssue.Id, owner.Id));
                Assert.True(await context.IssueStatusHistories.AnyAsync(h =>
                    h.IssueId == oldIssue.Id && h.Note == "Earlier rejection explanation"));
            }
            finally
            {
                context.ChangeTracker.Clear();
                await context.Database.EnsureDeletedAsync();
            }
        }

        private static User NewUser(string name, byte role)
        {
            return new User
            {
                FullName = name + " Test User",
                Email = name.ToLowerInvariant() + "@example.test",
                PasswordHash = "TEST_ONLY_PLACEHOLDER",
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
