#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryCycleName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SalaryCycles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "SalaryCycles");
        }
    }
}
