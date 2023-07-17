using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectMemberReportAttrs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectReportMemberAttributes",
                columns: table => new
                {
                    ProjectReportMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectReportMemberAttributes", x => new { x.ProjectReportMemberId, x.AttributeId });
                    table.ForeignKey(
                        name: "FK_ProjectReportMemberAttributes_Attributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "Attributes",
                        principalColumn: "AttributeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectReportMemberAttributes_ProjectReportMembers_ProjectReportMemberId",
                        column: x => x.ProjectReportMemberId,
                        principalTable: "ProjectReportMembers",
                        principalColumn: "ProjectReportMemberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectReportMemberAttributes_AttributeId",
                table: "ProjectReportMemberAttributes",
                column: "AttributeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectReportMemberAttributes");
        }
    }
}
