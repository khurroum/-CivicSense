using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CivicSense.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeLookupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssuePriorities",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuePriorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IssueStatuses",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IssueTypes",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentStatuses",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequestStatuses",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "IssuePriorities",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)1, "Low" },
                    { (byte)2, "Medium" },
                    { (byte)3, "High" },
                    { (byte)4, "Critical" }
                });

            migrationBuilder.InsertData(
                table: "IssueStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)1, "Reported" },
                    { (byte)2, "UnderReview" },
                    { (byte)3, "Assigned" },
                    { (byte)4, "InProgress" },
                    { (byte)5, "Resolved" },
                    { (byte)6, "Rejected" },
                    { (byte)7, "NeedsInformation" },
                    { (byte)8, "Reopened" }
                });

            migrationBuilder.InsertData(
                table: "IssueTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)1, "PublicCivic" },
                    { (byte)2, "PersonalPrivate" }
                });

            migrationBuilder.InsertData(
                table: "PaymentStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)1, "Pending" },
                    { (byte)2, "Processing" },
                    { (byte)3, "Completed" },
                    { (byte)4, "Failed" },
                    { (byte)5, "Refunded" },
                    { (byte)6, "Cancelled" }
                });

            migrationBuilder.InsertData(
                table: "ServiceRequestStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)1, "Pending" },
                    { (byte)2, "Accepted" },
                    { (byte)3, "InProgress" },
                    { (byte)4, "Completed" },
                    { (byte)5, "Cancelled" },
                    { (byte)6, "Rejected" }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)1, "Resident" },
                    { (byte)2, "Staff" },
                    { (byte)3, "Admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_Status",
                table: "ServiceRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Status",
                table: "PaymentTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_IssueStatusHistories_NewStatus",
                table: "IssueStatusHistories",
                column: "NewStatus");

            migrationBuilder.CreateIndex(
                name: "IX_IssueStatusHistories_OldStatus",
                table: "IssueStatusHistories",
                column: "OldStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Priority",
                table: "Issues",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Status",
                table: "Issues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AIAnalyses_SuggestedIssueType",
                table: "AIAnalyses",
                column: "SuggestedIssueType");

            migrationBuilder.CreateIndex(
                name: "IX_AIAnalyses_SuggestedPriority",
                table: "AIAnalyses",
                column: "SuggestedPriority");

            migrationBuilder.CreateIndex(
                name: "IX_IssuePriorities_Name",
                table: "IssuePriorities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IssueStatuses_Name",
                table: "IssueStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IssueTypes_Name",
                table: "IssueTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatuses_Name",
                table: "PaymentStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestStatuses_Name",
                table: "ServiceRequestStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_Name",
                table: "UserRoles",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AIAnalyses_IssuePriorities_SuggestedPriority",
                table: "AIAnalyses",
                column: "SuggestedPriority",
                principalTable: "IssuePriorities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AIAnalyses_IssueTypes_SuggestedIssueType",
                table: "AIAnalyses",
                column: "SuggestedIssueType",
                principalTable: "IssueTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_IssuePriorities_Priority",
                table: "Issues",
                column: "Priority",
                principalTable: "IssuePriorities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_IssueStatuses_Status",
                table: "Issues",
                column: "Status",
                principalTable: "IssueStatuses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_IssueTypes_IssueType",
                table: "Issues",
                column: "IssueType",
                principalTable: "IssueTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IssueStatusHistories_IssueStatuses_NewStatus",
                table: "IssueStatusHistories",
                column: "NewStatus",
                principalTable: "IssueStatuses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IssueStatusHistories_IssueStatuses_OldStatus",
                table: "IssueStatusHistories",
                column: "OldStatus",
                principalTable: "IssueStatuses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_PaymentStatuses_Status",
                table: "PaymentTransactions",
                column: "Status",
                principalTable: "PaymentStatuses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_ServiceRequestStatuses_Status",
                table: "ServiceRequests",
                column: "Status",
                principalTable: "ServiceRequestStatuses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserRoles_Role",
                table: "Users",
                column: "Role",
                principalTable: "UserRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIAnalyses_IssuePriorities_SuggestedPriority",
                table: "AIAnalyses");

            migrationBuilder.DropForeignKey(
                name: "FK_AIAnalyses_IssueTypes_SuggestedIssueType",
                table: "AIAnalyses");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_IssuePriorities_Priority",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_IssueStatuses_Status",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_IssueTypes_IssueType",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueStatusHistories_IssueStatuses_NewStatus",
                table: "IssueStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueStatusHistories_IssueStatuses_OldStatus",
                table: "IssueStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_PaymentStatuses_Status",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_ServiceRequestStatuses_Status",
                table: "ServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserRoles_Role",
                table: "Users");

            migrationBuilder.DropTable(
                name: "IssuePriorities");

            migrationBuilder.DropTable(
                name: "IssueStatuses");

            migrationBuilder.DropTable(
                name: "IssueTypes");

            migrationBuilder.DropTable(
                name: "PaymentStatuses");

            migrationBuilder.DropTable(
                name: "ServiceRequestStatuses");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_Status",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_Status",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_IssueStatusHistories_NewStatus",
                table: "IssueStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_IssueStatusHistories_OldStatus",
                table: "IssueStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_Issues_Priority",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_Status",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_AIAnalyses_SuggestedIssueType",
                table: "AIAnalyses");

            migrationBuilder.DropIndex(
                name: "IX_AIAnalyses_SuggestedPriority",
                table: "AIAnalyses");
        }
    }
}
