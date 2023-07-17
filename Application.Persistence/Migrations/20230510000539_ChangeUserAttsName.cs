using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserAttsName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordChangeTokenCreatedAt",
                table: "Users",
                newName: "PasswordChangeTokenLastSent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordChangeTokenLastSent",
                table: "Users",
                newName: "PasswordChangeTokenCreatedAt");
        }
    }
}
