using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_stats_measurement.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExportRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Theme = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedAnswer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpectedSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActualAnswer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RawText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SquareMeanRootError = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RelativeError = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AnswerIsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    SourceIsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prompts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Instruction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Periode = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    AnswerLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prompts_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModelResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromptId = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RawText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelResponses_Prompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "Prompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromptDimensions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromptId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptDimensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromptDimensions_Prompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "Prompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParsedModelResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelResponseId = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedModelResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParsedModelResponses_ModelResponses_ModelResponseId",
                        column: x => x.ModelResponseId,
                        principalTable: "ModelResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FactCheckResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParsedModelResponseId = table.Column<int>(type: "int", nullable: false),
                    SquareMeanRootError = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RelativeError = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AnswerIsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    SourceIsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactCheckResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactCheckResults_ParsedModelResponses_ParsedModelResponseId",
                        column: x => x.ParsedModelResponseId,
                        principalTable: "ParsedModelResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParsedModelResponseSources",
                columns: table => new
                {
                    ParsedModelResponseId = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    ExportRowId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedModelResponseSources", x => new { x.ParsedModelResponseId, x.SourceId });
                    table.ForeignKey(
                        name: "FK_ParsedModelResponseSources_ExportRows_ExportRowId",
                        column: x => x.ExportRowId,
                        principalTable: "ExportRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ParsedModelResponseSources_ParsedModelResponses_ParsedModelResponseId",
                        column: x => x.ParsedModelResponseId,
                        principalTable: "ParsedModelResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParsedModelResponseSources_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FactCheckResults_ParsedModelResponseId",
                table: "FactCheckResults",
                column: "ParsedModelResponseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModelResponses_PromptId",
                table: "ModelResponses",
                column: "PromptId");

            migrationBuilder.CreateIndex(
                name: "IX_ParsedModelResponses_ModelResponseId",
                table: "ParsedModelResponses",
                column: "ModelResponseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedModelResponseSources_ExportRowId",
                table: "ParsedModelResponseSources",
                column: "ExportRowId");

            migrationBuilder.CreateIndex(
                name: "IX_ParsedModelResponseSources_SourceId",
                table: "ParsedModelResponseSources",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_PromptDimensions_PromptId_Name",
                table: "PromptDimensions",
                columns: new[] { "PromptId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_SourceId",
                table: "Prompts",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Sources_Name_Url",
                table: "Sources",
                columns: new[] { "Name", "Url" },
                unique: true,
                filter: "[Name] IS NOT NULL AND [Url] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FactCheckResults");

            migrationBuilder.DropTable(
                name: "ParsedModelResponseSources");

            migrationBuilder.DropTable(
                name: "PromptDimensions");

            migrationBuilder.DropTable(
                name: "ExportRows");

            migrationBuilder.DropTable(
                name: "ParsedModelResponses");

            migrationBuilder.DropTable(
                name: "ModelResponses");

            migrationBuilder.DropTable(
                name: "Prompts");

            migrationBuilder.DropTable(
                name: "Sources");
        }
    }
}
