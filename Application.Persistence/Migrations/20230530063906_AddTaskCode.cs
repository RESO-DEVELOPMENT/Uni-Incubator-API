using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TaskCode",
                table: "ProjectReportMemberTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskCode",
                table: "ProjectReportMemberTasks");
        }
    }
}
