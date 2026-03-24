using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_stats_measurement.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParsedModelResponseSources_ExportRows_ExportRowId",
                table: "ParsedModelResponseSources");

            migrationBuilder.DropIndex(
                name: "IX_ParsedModelResponseSources_ExportRowId",
                table: "ParsedModelResponseSources");

            migrationBuilder.DropColumn(
                name: "ExportRowId",
                table: "ParsedModelResponseSources");

            migrationBuilder.AddColumn<string>(
                name: "ActualSource",
                table: "ExportRows",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualSource",
                table: "ExportRows");

            migrationBuilder.AddColumn<int>(
                name: "ExportRowId",
                table: "ParsedModelResponseSources",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedModelResponseSources_ExportRowId",
                table: "ParsedModelResponseSources",
                column: "ExportRowId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParsedModelResponseSources_ExportRows_ExportRowId",
                table: "ParsedModelResponseSources",
                column: "ExportRowId",
                principalTable: "ExportRows",
                principalColumn: "Id");
        }
    }
}
