using CivicSense.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicSense.DataAccess
{
    internal static class LookupModelConfiguration
    {
        internal static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRoleLookup>(entity =>
            {
                entity.ToTable("UserRoles");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id)
                    .HasConversion<byte>()
                    .ValueGeneratedNever();
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.HasData(
                    new UserRoleLookup { Id = UserRole.Resident, Name = "Resident" },
                    new UserRoleLookup { Id = UserRole.Staff, Name = "Staff" },
                    new UserRoleLookup { Id = UserRole.Admin, Name = "Admin" });
            });

            modelBuilder.Entity<IssueTypeLookup>(entity =>
            {
                entity.ToTable("IssueTypes");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id)
                    .HasConversion<byte>()
                    .ValueGeneratedNever();
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.HasData(
                    new IssueTypeLookup { Id = IssueType.PublicCivic, Name = "PublicCivic" },
                    new IssueTypeLookup { Id = IssueType.PersonalPrivate, Name = "PersonalPrivate" });
            });

            modelBuilder.Entity<IssueStatusLookup>(entity =>
            {
                entity.ToTable("IssueStatuses");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id)
                    .HasConversion<byte>()
                    .ValueGeneratedNever();
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.HasData(
                    new IssueStatusLookup { Id = IssueStatus.Reported, Name = "Reported" },
                    new IssueStatusLookup { Id = IssueStatus.UnderReview, Name = "UnderReview" },
                    new IssueStatusLookup { Id = IssueStatus.Assigned, Name = "Assigned" },
                    new IssueStatusLookup { Id = IssueStatus.InProgress, Name = "InProgress" },
                    new IssueStatusLookup { Id = IssueStatus.Resolved, Name = "Resolved" },
                    new IssueStatusLookup { Id = IssueStatus.Rejected, Name = "Rejected" },
                    new IssueStatusLookup { Id = IssueStatus.NeedsInformation, Name = "NeedsInformation" },
                    new IssueStatusLookup { Id = IssueStatus.Reopened, Name = "Reopened" });
            });

            modelBuilder.Entity<IssuePriorityLookup>(entity =>
            {
                entity.ToTable("IssuePriorities");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id)
                    .HasConversion<byte>()
                    .ValueGeneratedNever();
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.HasData(
                    new IssuePriorityLookup { Id = IssuePriority.Low, Name = "Low" },
                    new IssuePriorityLookup { Id = IssuePriority.Medium, Name = "Medium" },
                    new IssuePriorityLookup { Id = IssuePriority.High, Name = "High" },
                    new IssuePriorityLookup { Id = IssuePriority.Critical, Name = "Critical" });
            });

            modelBuilder.Entity<ServiceRequestStatusLookup>(entity =>
            {
                entity.ToTable("ServiceRequestStatuses");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id)
                    .HasConversion<byte>()
                    .ValueGeneratedNever();
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.HasData(
                    new ServiceRequestStatusLookup { Id = ServiceRequestStatus.Pending, Name = "Pending" },
                    new ServiceRequestStatusLookup { Id = ServiceRequestStatus.Accepted, Name = "Accepted" },
                    new ServiceRequestStatusLookup { Id = ServiceRequestStatus.InProgress, Name = "InProgress" },
                    new ServiceRequestStatusLookup { Id = ServiceRequestStatus.Completed, Name = "Completed" },
                    new ServiceRequestStatusLookup { Id = ServiceRequestStatus.Cancelled, Name = "Cancelled" },
                    new ServiceRequestStatusLookup { Id = ServiceRequestStatus.Rejected, Name = "Rejected" });
            });

            modelBuilder.Entity<PaymentStatusLookup>(entity =>
            {
                entity.ToTable("PaymentStatuses");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id)
                    .HasConversion<byte>()
                    .ValueGeneratedNever();
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.HasData(
                    new PaymentStatusLookup { Id = PaymentStatus.Pending, Name = "Pending" },
                    new PaymentStatusLookup { Id = PaymentStatus.Processing, Name = "Processing" },
                    new PaymentStatusLookup { Id = PaymentStatus.Completed, Name = "Completed" },
                    new PaymentStatusLookup { Id = PaymentStatus.Failed, Name = "Failed" },
                    new PaymentStatusLookup { Id = PaymentStatus.Refunded, Name = "Refunded" },
                    new PaymentStatusLookup { Id = PaymentStatus.Cancelled, Name = "Cancelled" });
            });

            // Use the existing tinyint columns as foreign keys.
            // Nullable history and AI fields remain nullable.
            modelBuilder.Entity<User>()
                .HasOne<UserRoleLookup>()
                .WithMany()
                .HasForeignKey(x => x.Role)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Issue>()
                .HasOne<IssueTypeLookup>()
                .WithMany()
                .HasForeignKey(x => x.IssueType)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Issue>()
                .HasOne<IssueStatusLookup>()
                .WithMany()
                .HasForeignKey(x => x.Status)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Issue>()
                .HasOne<IssuePriorityLookup>()
                .WithMany()
                .HasForeignKey(x => x.Priority)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<IssueStatusHistory>()
                .HasOne<IssueStatusLookup>()
                .WithMany()
                .HasForeignKey(x => x.OldStatus)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<IssueStatusHistory>()
                .HasOne<IssueStatusLookup>()
                .WithMany()
                .HasForeignKey(x => x.NewStatus)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AIAnalysis>()
                .HasOne<IssueTypeLookup>()
                .WithMany()
                .HasForeignKey(x => x.SuggestedIssueType)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AIAnalysis>()
                .HasOne<IssuePriorityLookup>()
                .WithMany()
                .HasForeignKey(x => x.SuggestedPriority)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne<ServiceRequestStatusLookup>()
                .WithMany()
                .HasForeignKey(x => x.Status)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaymentTransaction>()
                .HasOne<PaymentStatusLookup>()
                .WithMany()
                .HasForeignKey(x => x.Status)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
