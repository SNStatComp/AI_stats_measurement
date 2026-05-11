using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_stats_measurement.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationExportModelResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "ModelResponses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModelResponseId",
                table: "ExportRows",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModelResponses_JobId",
                table: "ModelResponses",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportRows_ModelResponseId",
                table: "ExportRows",
                column: "ModelResponseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExportRows_ModelResponses_ModelResponseId",
                table: "ExportRows",
                column: "ModelResponseId",
                principalTable: "ModelResponses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelResponses_LlmJobs_JobId",
                table: "ModelResponses",
                column: "JobId",
                principalTable: "LlmJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExportRows_ModelResponses_ModelResponseId",
                table: "ExportRows");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelResponses_LlmJobs_JobId",
                table: "ModelResponses");

            migrationBuilder.DropIndex(
                name: "IX_ModelResponses_JobId",
                table: "ModelResponses");

            migrationBuilder.DropIndex(
                name: "IX_ExportRows_ModelResponseId",
                table: "ExportRows");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "ModelResponses");

            migrationBuilder.DropColumn(
                name: "ModelResponseId",
                table: "ExportRows");
        }
    }
}
