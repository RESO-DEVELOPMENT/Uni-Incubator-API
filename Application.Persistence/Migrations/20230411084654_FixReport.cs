#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectBonusReportItems");

            migrationBuilder.DropTable(
                name: "ProjectBonusReports");

            migrationBuilder.AddColumn<string>(
                name: "BonusReason",
                table: "ProjectReportMemberTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TaskBonus",
                table: "ProjectReportMemberTasks",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TaskPoint",
                table: "ProjectReportMemberTasks",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "ProjectReportMemberAttribute",
                columns: table => new
                {
                    ProjectReportMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectReportMemberAttribute", x => new { x.ProjectReportMemberId, x.AttributeId });
                    table.ForeignKey(
                        name: "FK_ProjectReportMemberAttribute_Attributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "Attributes",
                        principalColumn: "AttributeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectReportMemberAttribute_ProjectReportMembers_ProjectReportMemberId",
                        column: x => x.ProjectReportMemberId,
                        principalTable: "ProjectReportMembers",
                        principalColumn: "ProjectReportMemberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectReportMemberAttribute_AttributeId",
                table: "ProjectReportMemberAttribute",
                column: "AttributeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectReportMemberAttribute");

            migrationBuilder.DropColumn(
                name: "BonusReason",
                table: "ProjectReportMemberTasks");

            migrationBuilder.DropColumn(
                name: "TaskBonus",
                table: "ProjectReportMemberTasks");

            migrationBuilder.DropColumn(
                name: "TaskPoint",
                table: "ProjectReportMemberTasks");

            migrationBuilder.CreateTable(
                name: "ProjectBonusReports",
                columns: table => new
                {
                    ProjectBonusReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalaryCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectBonusReports", x => x.ProjectBonusReportId);
                    table.ForeignKey(
                        name: "FK_ProjectBonusReports_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectBonusReports_SalaryCycles_SalaryCycleId",
                        column: x => x.SalaryCycleId,
                        principalTable: "SalaryCycles",
                        principalColumn: "SalaryCycleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectBonusReportItems",
                columns: table => new
                {
                    ProjectBonusReportItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectBonusReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectReportMemberTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectBonusReportItems", x => x.ProjectBonusReportItemId);
                    table.ForeignKey(
                        name: "FK_ProjectBonusReportItems_ProjectBonusReports_ProjectBonusReportId",
                        column: x => x.ProjectBonusReportId,
                        principalTable: "ProjectBonusReports",
                        principalColumn: "ProjectBonusReportId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectBonusReportItems_ProjectReportMemberTasks_ProjectReportMemberTaskId",
                        column: x => x.ProjectReportMemberTaskId,
                        principalTable: "ProjectReportMemberTasks",
                        principalColumn: "ProjectReportMemberTaskId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBonusReportItems_ProjectBonusReportId",
                table: "ProjectBonusReportItems",
                column: "ProjectBonusReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBonusReportItems_ProjectReportMemberTaskId",
                table: "ProjectBonusReportItems",
                column: "ProjectReportMemberTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBonusReports_ProjectId",
                table: "ProjectBonusReports",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBonusReports_SalaryCycleId",
                table: "ProjectBonusReports",
                column: "SalaryCycleId");
        }
    }
}
