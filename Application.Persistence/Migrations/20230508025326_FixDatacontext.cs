using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixDatacontext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayslipAttribute_Attributes_AttributeId",
                table: "PayslipAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_PayslipAttribute_Payslip_PayslipId",
                table: "PayslipAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_PayslipItemAttribute_Attributes_AttributeId",
                table: "PayslipItemAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_PayslipItemAttribute_PayslipItem_PayslipItemId",
                table: "PayslipItemAttribute");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PayslipItemAttribute",
                table: "PayslipItemAttribute");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PayslipAttribute",
                table: "PayslipAttribute");

            migrationBuilder.RenameTable(
                name: "PayslipItemAttribute",
                newName: "PayslipItemAttributes");

            migrationBuilder.RenameTable(
                name: "PayslipAttribute",
                newName: "PayslipAttributes");

            migrationBuilder.RenameIndex(
                name: "IX_PayslipItemAttribute_AttributeId",
                table: "PayslipItemAttributes",
                newName: "IX_PayslipItemAttributes_AttributeId");

            migrationBuilder.RenameIndex(
                name: "IX_PayslipAttribute_AttributeId",
                table: "PayslipAttributes",
                newName: "IX_PayslipAttributes_AttributeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PayslipItemAttributes",
                table: "PayslipItemAttributes",
                columns: new[] { "PayslipItemId", "AttributeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PayslipAttributes",
                table: "PayslipAttributes",
                columns: new[] { "PayslipId", "AttributeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PayslipAttributes_Attributes_AttributeId",
                table: "PayslipAttributes",
                column: "AttributeId",
                principalTable: "Attributes",
                principalColumn: "AttributeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayslipAttributes_Payslip_PayslipId",
                table: "PayslipAttributes",
                column: "PayslipId",
                principalTable: "Payslip",
                principalColumn: "PayslipId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayslipItemAttributes_Attributes_AttributeId",
                table: "PayslipItemAttributes",
                column: "AttributeId",
                principalTable: "Attributes",
                principalColumn: "AttributeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayslipItemAttributes_PayslipItem_PayslipItemId",
                table: "PayslipItemAttributes",
                column: "PayslipItemId",
                principalTable: "PayslipItem",
                principalColumn: "PayslipItemId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayslipAttributes_Attributes_AttributeId",
                table: "PayslipAttributes");

            migrationBuilder.DropForeignKey(
                name: "FK_PayslipAttributes_Payslip_PayslipId",
                table: "PayslipAttributes");

            migrationBuilder.DropForeignKey(
                name: "FK_PayslipItemAttributes_Attributes_AttributeId",
                table: "PayslipItemAttributes");

            migrationBuilder.DropForeignKey(
                name: "FK_PayslipItemAttributes_PayslipItem_PayslipItemId",
                table: "PayslipItemAttributes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PayslipItemAttributes",
                table: "PayslipItemAttributes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PayslipAttributes",
                table: "PayslipAttributes");

            migrationBuilder.RenameTable(
                name: "PayslipItemAttributes",
                newName: "PayslipItemAttribute");

            migrationBuilder.RenameTable(
                name: "PayslipAttributes",
                newName: "PayslipAttribute");

            migrationBuilder.RenameIndex(
                name: "IX_PayslipItemAttributes_AttributeId",
                table: "PayslipItemAttribute",
                newName: "IX_PayslipItemAttribute_AttributeId");

            migrationBuilder.RenameIndex(
                name: "IX_PayslipAttributes_AttributeId",
                table: "PayslipAttribute",
                newName: "IX_PayslipAttribute_AttributeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PayslipItemAttribute",
                table: "PayslipItemAttribute",
                columns: new[] { "PayslipItemId", "AttributeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PayslipAttribute",
                table: "PayslipAttribute",
                columns: new[] { "PayslipId", "AttributeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PayslipAttribute_Attributes_AttributeId",
                table: "PayslipAttribute",
                column: "AttributeId",
                principalTable: "Attributes",
                principalColumn: "AttributeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayslipAttribute_Payslip_PayslipId",
                table: "PayslipAttribute",
                column: "PayslipId",
                principalTable: "Payslip",
                principalColumn: "PayslipId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayslipItemAttribute_Attributes_AttributeId",
                table: "PayslipItemAttribute",
                column: "AttributeId",
                principalTable: "Attributes",
                principalColumn: "AttributeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayslipItemAttribute_PayslipItem_PayslipItemId",
                table: "PayslipItemAttribute",
                column: "PayslipItemId",
                principalTable: "PayslipItem",
                principalColumn: "PayslipItemId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
