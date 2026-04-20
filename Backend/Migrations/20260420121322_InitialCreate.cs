using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Theme = table.Column<string>(type: "text", nullable: false),
                    Question = table.Column<string>(type: "text", nullable: false),
                    ExpectedAnswer = table.Column<decimal>(type: "numeric", nullable: false),
                    ExpectedSource = table.Column<string>(type: "text", nullable: false),
                    ActualAnswer = table.Column<decimal>(type: "numeric", nullable: false),
                    ActualSource = table.Column<List<int>>(type: "integer[]", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    RawText = table.Column<string>(type: "text", nullable: true),
                    Exception = table.Column<string>(type: "text", nullable: true),
                    SquareMeanRootError = table.Column<decimal>(type: "numeric", nullable: false),
                    RelativeError = table.Column<decimal>(type: "numeric", nullable: false),
                    AnswerIsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    SourceIsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prompts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    Instruction = table.Column<string>(type: "text", nullable: false),
                    Theme = table.Column<string>(type: "text", nullable: false),
                    Periode = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Question = table.Column<string>(type: "text", nullable: false),
                    Answer = table.Column<decimal>(type: "numeric", nullable: false),
                    SourceId = table.Column<int>(type: "integer", nullable: false),
                    AnswerLocation = table.Column<string>(type: "text", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PromptId = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    RawText = table.Column<string>(type: "text", nullable: true),
                    Exception = table.Column<string>(type: "text", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PromptId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModelResponseId = table.Column<int>(type: "integer", nullable: false),
                    Answer = table.Column<decimal>(type: "numeric", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParsedModelResponseId = table.Column<int>(type: "integer", nullable: false),
                    AbsoluteError = table.Column<decimal>(type: "numeric", nullable: false),
                    RelativeError = table.Column<decimal>(type: "numeric", nullable: false),
                    AnswerIsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    SourceIsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    Abstained = table.Column<bool>(type: "boolean", nullable: false)
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
                    ParsedModelResponseId = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedModelResponseSources", x => new { x.ParsedModelResponseId, x.SourceId });
                    table.ForeignKey(
                        name: "FK_ParsedModelResponseSources_ParsedModelResponses_ParsedModel~",
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
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExportRows");

            migrationBuilder.DropTable(
                name: "FactCheckResults");

            migrationBuilder.DropTable(
                name: "ParsedModelResponseSources");

            migrationBuilder.DropTable(
                name: "PromptDimensions");

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
