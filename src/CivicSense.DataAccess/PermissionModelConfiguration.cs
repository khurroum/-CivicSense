using CivicSense.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicSense.DataAccess
{
    internal static class PermissionModelConfiguration
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permissions");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.DisplayText)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.HasIndex(x => x.Name)
                    .IsUnique();

                entity.HasOne<Permission>()
                    .WithMany()
                    .HasForeignKey(x => x.ParentId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasData(
                    new Permission
                    {
                        Id = 1,
                        Name = PermissionCodes.ManageCategories,
                        DisplayText = "Manage categories",
                        IsActive = true
                    },
                    new Permission
                    {
                        Id = -100,
                        Name = PermissionCodes.ManageAccess,
                        DisplayText = "Manage users and permissions",
                        IsActive = true
                    });
            });

            modelBuilder.Entity<PermissionGroup>(entity =>
            {
                entity.ToTable("PermissionGroups");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Tag)
                    .HasMaxLength(500);

                entity.HasIndex(x => x.Name)
                    .IsUnique();

                entity.HasIndex(x => x.DefaultRoleId)
                    .IsUnique()
                    .HasFilter("[DefaultRoleId] IS NOT NULL");

                entity.HasOne<UserRoleLookup>()
                    .WithMany()
                    .HasForeignKey(x => x.DefaultRoleId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasData(
                    new PermissionGroup
                    {
                        Id = 1,
                        Name = "Default Resident",
                        DefaultRoleId = UserRoleLookup.Resident
                    },
                    new PermissionGroup
                    {
                        Id = 2,
                        Name = "Default Staff",
                        DefaultRoleId = UserRoleLookup.Staff
                    },
                    new PermissionGroup
                    {
                        Id = 3,
                        Name = "Default Admin",
                        DefaultRoleId = UserRoleLookup.Admin
                    });
            });

            modelBuilder.Entity<PermissionGroupMap>(entity =>
            {
                entity.ToTable("PermissionGroupMaps");

                entity.HasKey(x => x.Id);

                entity.HasIndex(x => new
                {
                    x.PermissionGroupId,
                    x.PermissionId
                }).IsUnique();

                entity.HasOne<PermissionGroup>()
                    .WithMany()
                    .HasForeignKey(x => x.PermissionGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Permission>()
                    .WithMany()
                    .HasForeignKey(x => x.PermissionId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasData(
                    new PermissionGroupMap
                    {
                        Id = 1,
                        PermissionGroupId = 3,
                        PermissionId = 1,
                        IsActive = true
                    },
                    new PermissionGroupMap
                    {
                        Id = -100,
                        PermissionGroupId = 3,
                        PermissionId = -100,
                        IsActive = true
                    });
            });

            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.ToTable("UserPermissions", table =>
                    table.HasCheckConstraint(
                        "CK_UserPermissions_ExactlyOneTarget",
                        "([PermissionId] IS NOT NULL AND " +
                        "[PermissionGroupId] IS NULL) OR " +
                        "([PermissionId] IS NULL AND " +
                        "[PermissionGroupId] IS NOT NULL AND " +
                        "[IsAllowed] = 1)"));

                entity.HasKey(x => x.Id);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Permission>()
                    .WithMany()
                    .HasForeignKey(x => x.PermissionId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<PermissionGroup>()
                    .WithMany()
                    .HasForeignKey(x => x.PermissionGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.PermissionId
                })
                .IsUnique()
                .HasFilter("[PermissionId] IS NOT NULL");

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.PermissionGroupId
                })
                .IsUnique()
                .HasFilter("[PermissionGroupId] IS NOT NULL");
            });
        }
    }
}