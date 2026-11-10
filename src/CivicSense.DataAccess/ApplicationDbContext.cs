
using CivicSense.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicSense.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // All 14 Entities
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Issue> Issues => Set<Issue>();
        public DbSet<IssueImage> IssueImages => Set<IssueImage>();
        public DbSet<IssueAssignment> IssueAssignments => Set<IssueAssignment>();
        public DbSet<IssueStatusHistory> IssueStatusHistories => Set<IssueStatusHistory>();
        public DbSet<IssueComment> IssueComments => Set<IssueComment>();
        public DbSet<IssueConfirmation> IssueConfirmations => Set<IssueConfirmation>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AIAnalysis> AIAnalyses => Set<AIAnalysis>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();
        public DbSet<ServiceProvider> ServiceProviders => Set<ServiceProvider>();
        public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. USER
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.FullName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                entity.Property(x => x.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(x => x.ProfileImagePath)
                    .HasMaxLength(500);

                entity.Property(x => x.Role)
                    .HasConversion<byte>();

                entity.Property(x => x.CreatedAt)
                    .HasColumnType("datetime2");
            });

            // 2. CATEGORY
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(x => x.Name)
                    .IsUnique();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);
            });

            // 3. ISSUE
            modelBuilder.Entity<Issue>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Description)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.Property(x => x.Address)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.Latitude)
                    .HasPrecision(9, 6);

                entity.Property(x => x.Longitude)
                    .HasPrecision(9, 6);

                entity.Property(x => x.IssueType)
                    .HasConversion<byte>();

                entity.Property(x => x.Status)
                    .HasConversion<byte>();

                entity.Property(x => x.Priority)
                    .HasConversion<byte>();

                entity.HasOne(x => x.Reporter)
                    .WithMany(x => x.Issues)
                    .HasForeignKey(x => x.ReporterId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.Category)
                    .WithMany(x => x.Issues)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new
                {
                    x.IssueType,
                    x.Status,
                    x.CreatedAt
                });

                entity.HasIndex(x => x.ReporterId);

                entity.HasIndex(x => x.CategoryId);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Issues_Latitude",
                        "[Latitude] IS NULL OR [Latitude] BETWEEN -90 AND 90");

                    t.HasCheckConstraint(
                        "CK_Issues_Longitude",
                        "[Longitude] IS NULL OR [Longitude] BETWEEN -180 AND 180");
                });
            });

            // 4. ISSUE IMAGE
            modelBuilder.Entity<IssueImage>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(x => x.FilePath)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.ContentType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(x => x.Issue)
                    .WithMany(x => x.Images)
                    .HasForeignKey(x => x.IssueId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.UploadedBy)
                    .WithMany()
                    .HasForeignKey(x => x.UploadedById)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // 5. ISSUE ASSIGNMENT
            modelBuilder.Entity<IssueAssignment>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.AssignmentNote)
                    .HasMaxLength(1000);

                entity.HasOne(x => x.Issue)
                    .WithMany(x => x.Assignments)
                    .HasForeignKey(x => x.IssueId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.Staff)
                    .WithMany(x => x.StaffAssignments)
                    .HasForeignKey(x => x.StaffId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.AssignedBy)
                    .WithMany()
                    .HasForeignKey(x => x.AssignedById)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => x.IssueId)
                    .IsUnique()
                    .HasFilter("[IsActive] = 1");
            });

            // 6. ISSUE STATUS HISTORY
            modelBuilder.Entity<IssueStatusHistory>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.OldStatus)
                    .HasConversion<byte?>();

                entity.Property(x => x.NewStatus)
                    .HasConversion<byte>();

                entity.Property(x => x.Note)
                    .HasMaxLength(1000);

                entity.HasOne(x => x.Issue)
                    .WithMany(x => x.StatusHistories)
                    .HasForeignKey(x => x.IssueId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.ChangedBy)
                    .WithMany()
                    .HasForeignKey(x => x.ChangedById)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new
                {
                    x.IssueId,
                    x.ChangedAt
                });
            });

            // 7. ISSUE COMMENT
            modelBuilder.Entity<IssueComment>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Content)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.HasOne(x => x.Issue)
                    .WithMany(x => x.Comments)
                    .HasForeignKey(x => x.IssueId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // 8. ISSUE CONFIRMATION
            modelBuilder.Entity<IssueConfirmation>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Issue)
                    .WithMany(x => x.Confirmations)
                    .HasForeignKey(x => x.IssueId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new
                {
                    x.IssueId,
                    x.UserId
                }).IsUnique();
            });

            // 9. NOTIFICATION
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Message)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(x => x.NotificationType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.Notifications)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.Issue)
                    .WithMany()
                    .HasForeignKey(x => x.IssueId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.IsRead,
                    x.CreatedAt
                });
            });

            // 10. AI ANALYSIS
            modelBuilder.Entity<AIAnalysis>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Provider)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.ModelName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.SuggestedSummary)
                    .HasMaxLength(2000);

                entity.Property(x => x.Explanation)
                    .HasMaxLength(4000);

                entity.Property(x => x.ConfidenceScore)
                    .HasPrecision(5, 4);

                entity.Property(x => x.SuggestedIssueType)
                    .HasConversion<byte?>();

                entity.Property(x => x.SuggestedPriority)
                    .HasConversion<byte?>();

                entity.HasOne(x => x.Issue)
                    .WithMany(x => x.AIAnalyses)
                    .HasForeignKey(x => x.IssueId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.SuggestedCategory)
                    .WithMany()
                    .HasForeignKey(x => x.SuggestedCategoryId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.ReviewedBy)
                    .WithMany()
                    .HasForeignKey(x => x.ReviewedById)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.ToTable(t =>
                    t.HasCheckConstraint(
                        "CK_AIAnalyses_ConfidenceScore",
                        "[ConfidenceScore] IS NULL OR [ConfidenceScore] BETWEEN 0 AND 1"));
            });

            // 11. FEEDBACK
            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Comment)
                    .HasMaxLength(2000);

                entity.HasOne(x => x.Issue)
                    .WithMany(x => x.Feedbacks)
                    .HasForeignKey(x => x.IssueId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.Feedbacks)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new
                {
                    x.IssueId,
                    x.UserId
                }).IsUnique();

                entity.ToTable(t =>
                    t.HasCheckConstraint(
                        "CK_Feedbacks_Rating",
                        "[Rating] BETWEEN 1 AND 5"));
            });

            // 12. SERVICE PROVIDER
            modelBuilder.Entity<ServiceProvider>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.BusinessName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.ServiceType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Description)
                    .HasMaxLength(2000);

                entity.Property(x => x.ContactNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(x => x.ServiceArea)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.StartingPrice)
                    .HasPrecision(18, 2);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => x.UserId)
                    .IsUnique();

                entity.ToTable(t =>
                    t.HasCheckConstraint(
                        "CK_ServiceProviders_StartingPrice",
                        "[StartingPrice] IS NULL OR [StartingPrice] >= 0"));
            });

            // 13. SERVICE REQUEST
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Description)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(x => x.ServiceAddress)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.EstimatedCost)
                    .HasPrecision(18, 2);

                entity.Property(x => x.Status)
                    .HasConversion<byte>();

                entity.HasOne(x => x.User)
                    .WithMany(x => x.ServiceRequests)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.ServiceProvider)
                    .WithMany(x => x.ServiceRequests)
                    .HasForeignKey(x => x.ServiceProviderId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.Issue)
                    .WithMany(x => x.ServiceRequests)
                    .HasForeignKey(x => x.IssueId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new
                {
                    x.ServiceProviderId,
                    x.Status
                });

                entity.ToTable(t =>
                    t.HasCheckConstraint(
                        "CK_ServiceRequests_EstimatedCost",
                        "[EstimatedCost] IS NULL OR [EstimatedCost] >= 0"));
            });

            // 14. PAYMENT TRANSACTION
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Amount)
                    .HasPrecision(18, 2);

                entity.Property(x => x.Currency)
                    .IsRequired()
                    .HasMaxLength(3);

                entity.Property(x => x.PaymentMethod)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.GatewayTransactionId)
                    .HasMaxLength(200);

                entity.Property(x => x.Status)
                    .HasConversion<byte>();

                entity.HasOne(x => x.ServiceRequest)
                    .WithMany(x => x.Payments)
                    .HasForeignKey(x => x.ServiceRequestId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => x.GatewayTransactionId)
                    .IsUnique()
                    .HasFilter("[GatewayTransactionId] IS NOT NULL");

                entity.ToTable(t =>
                    t.HasCheckConstraint(
                        "CK_PaymentTransactions_Amount",
                        "[Amount] > 0"));
            });
        }
    }
}
