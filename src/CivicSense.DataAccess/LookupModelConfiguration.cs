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
                    .ValueGeneratedNever();
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.HasData(
                    new UserRoleLookup { Id = UserRoleLookup.Resident, Name = "Resident" },
                    new UserRoleLookup { Id = UserRoleLookup.Staff, Name = "Staff" },
                    new UserRoleLookup { Id = UserRoleLookup.Admin, Name = "Admin" });
            });

            modelBuilder.Entity<IssueTypeLookup>(entity =>
            {
                entity.ToTable("IssueTypes");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id)
                    .ValueGeneratedNever();
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.HasData(
                    new IssueTypeLookup { Id = IssueTypeLookup.PublicCivic, Name = "PublicCivic" },
                    new IssueTypeLookup { Id = IssueTypeLookup.PersonalPrivate, Name = "PersonalPrivate" });
            });

            modelBuilder.Entity<IssueStatusLookup>(entity =>
            {
                entity.ToTable("IssueStatuses");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id)
                    .ValueGeneratedNever();
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.HasData(
                    new IssueStatusLookup { Id = IssueStatusLookup.Reported, Name = "Reported" },
                    new IssueStatusLookup { Id = IssueStatusLookup.UnderReview, Name = "UnderReview" },
                    new IssueStatusLookup { Id = IssueStatusLookup.Assigned, Name = "Assigned" },
                    new IssueStatusLookup { Id = IssueStatusLookup.InProgress, Name = "InProgress" },
                    new IssueStatusLookup { Id = IssueStatusLookup.Resolved, Name = "Resolved" },
                    new IssueStatusLookup { Id = IssueStatusLookup.Rejected, Name = "Rejected" },
                    new IssueStatusLookup { Id = IssueStatusLookup.NeedsInformation, Name = "NeedsInformation" },
                    new IssueStatusLookup { Id = IssueStatusLookup.Reopened, Name = "Reopened" });
            });

            modelBuilder.Entity<IssuePriorityLookup>(entity =>
            {
                entity.ToTable("IssuePriorities");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id)
                    .ValueGeneratedNever();
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.HasData(
                    new IssuePriorityLookup { Id = IssuePriorityLookup.Low, Name = "Low" },
                    new IssuePriorityLookup { Id = IssuePriorityLookup.Medium, Name = "Medium" },
                    new IssuePriorityLookup { Id = IssuePriorityLookup.High, Name = "High" },
                    new IssuePriorityLookup { Id = IssuePriorityLookup.Critical, Name = "Critical" });
            });

            modelBuilder.Entity<ServiceRequestStatusLookup>(entity =>
            {
                entity.ToTable("ServiceRequestStatuses");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id)
                    .ValueGeneratedNever();
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.HasData(
                    new ServiceRequestStatusLookup { Id = ServiceRequestStatusLookup.Pending, Name = "Pending" },
                    new ServiceRequestStatusLookup { Id = ServiceRequestStatusLookup.Accepted, Name = "Accepted" },
                    new ServiceRequestStatusLookup { Id = ServiceRequestStatusLookup.InProgress, Name = "InProgress" },
                    new ServiceRequestStatusLookup { Id = ServiceRequestStatusLookup.Completed, Name = "Completed" },
                    new ServiceRequestStatusLookup { Id = ServiceRequestStatusLookup.Cancelled, Name = "Cancelled" },
                    new ServiceRequestStatusLookup { Id = ServiceRequestStatusLookup.Rejected, Name = "Rejected" });
            });

            modelBuilder.Entity<PaymentStatusLookup>(entity =>
            {
                entity.ToTable("PaymentStatuses");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id)
                    .ValueGeneratedNever();
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.HasData(
                    new PaymentStatusLookup { Id = PaymentStatusLookup.Pending, Name = "Pending" },
                    new PaymentStatusLookup { Id = PaymentStatusLookup.Processing, Name = "Processing" },
                    new PaymentStatusLookup { Id = PaymentStatusLookup.Completed, Name = "Completed" },
                    new PaymentStatusLookup { Id = PaymentStatusLookup.Failed, Name = "Failed" },
                    new PaymentStatusLookup { Id = PaymentStatusLookup.Refunded, Name = "Refunded" },
                    new PaymentStatusLookup { Id = PaymentStatusLookup.Cancelled, Name = "Cancelled" });
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
