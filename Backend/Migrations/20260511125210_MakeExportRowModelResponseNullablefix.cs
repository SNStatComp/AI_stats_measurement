using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_stats_measurement.Backend.Migrations
{
    /// <inheritdoc />
    public partial class MakeExportRowModelResponseNullablefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExportRows_ModelResponses_ModelResponseId",
                table: "ExportRows");

            migrationBuilder.AlterColumn<int>(
                name: "ModelResponseId",
                table: "ExportRows",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_ExportRows_ModelResponses_ModelResponseId",
                table: "ExportRows",
                column: "ModelResponseId",
                principalTable: "ModelResponses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExportRows_ModelResponses_ModelResponseId",
                table: "ExportRows");

            migrationBuilder.AlterColumn<int>(
                name: "ModelResponseId",
                table: "ExportRows",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExportRows_ModelResponses_ModelResponseId",
                table: "ExportRows",
                column: "ModelResponseId",
                principalTable: "ModelResponses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
