using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixLevelAndTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Loyal",
                table: "Levels");

            migrationBuilder.AddColumn<double>(
                name: "TaskEffort",
                table: "ProjectReportMemberTasks",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskEffort",
                table: "ProjectReportMemberTasks");

            migrationBuilder.AddColumn<double>(
                name: "Loyal",
                table: "Levels",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
