using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivicSense.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessManagementPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "DisplayText", "IsActive", "Name", "ParentId" },
                values: new object[] { -100, "Manage users and permissions", true, "Access.Manage", null });

            migrationBuilder.InsertData(
                table: "PermissionGroupMaps",
                columns: new[] { "Id", "IsActive", "PermissionGroupId", "PermissionId" },
                values: new object[] { -100, true, 3, -100 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermissionGroupMaps",
                keyColumn: "Id",
                keyValue: -100);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: -100);
        }
    }
}
