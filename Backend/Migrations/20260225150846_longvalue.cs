using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_stats_measurement.Migrations
{
    /// <inheritdoc />
    public partial class longvalue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromptDimension_Prompts_PromptId",
                table: "PromptDimension");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromptDimension",
                table: "PromptDimension");

            migrationBuilder.RenameTable(
                name: "PromptDimension",
                newName: "PromptDimensions");

            migrationBuilder.RenameIndex(
                name: "IX_PromptDimension_PromptId_Name",
                table: "PromptDimensions",
                newName: "IX_PromptDimensions_PromptId_Name");

            migrationBuilder.AlterColumn<long>(
                name: "Answer",
                table: "Prompts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromptDimensions",
                table: "PromptDimensions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PromptDimensions_Prompts_PromptId",
                table: "PromptDimensions",
                column: "PromptId",
                principalTable: "Prompts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromptDimensions_Prompts_PromptId",
                table: "PromptDimensions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromptDimensions",
                table: "PromptDimensions");

            migrationBuilder.RenameTable(
                name: "PromptDimensions",
                newName: "PromptDimension");

            migrationBuilder.RenameIndex(
                name: "IX_PromptDimensions_PromptId_Name",
                table: "PromptDimension",
                newName: "IX_PromptDimension_PromptId_Name");

            migrationBuilder.AlterColumn<string>(
                name: "Answer",
                table: "Prompts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromptDimension",
                table: "PromptDimension",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PromptDimension_Prompts_PromptId",
                table: "PromptDimension",
                column: "PromptId",
                principalTable: "Prompts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
