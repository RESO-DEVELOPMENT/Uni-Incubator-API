using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUdAttrs1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberAttributes");

            migrationBuilder.DropTable(
                name: "ProjectReportMemberAttribute");

            migrationBuilder.AddColumn<int>(
                name: "VoucherType",
                table: "Vouchers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoucherType",
                table: "Vouchers");

            migrationBuilder.CreateTable(
                name: "MemberAttributes",
                columns: table => new
                {
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberAttributes", x => new { x.MemberId, x.AttributeId });
                    table.ForeignKey(
                        name: "FK_MemberAttributes_Attributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "Attributes",
                        principalColumn: "AttributeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberAttributes_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "IX_MemberAttributes_AttributeId",
                table: "MemberAttributes",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectReportMemberAttribute_AttributeId",
                table: "ProjectReportMemberAttribute",
                column: "AttributeId");
        }
    }
}
