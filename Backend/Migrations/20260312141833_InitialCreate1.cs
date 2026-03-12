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
            migrationBuilder.RenameColumn(
                name: "IsCorrect",
                table: "FactCheckResults",
                newName: "SourceIsCorrect");

            migrationBuilder.RenameColumn(
                name: "IsCorrect",
                table: "ExportRows",
                newName: "SourceIsCorrect");

            migrationBuilder.AddColumn<bool>(
                name: "AnswerIsCorrect",
                table: "FactCheckResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageAnswer",
                table: "FactCheckResults",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageAnswerCorrectness",
                table: "FactCheckResults",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRelativeError",
                table: "FactCheckResults",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageSourceCorrectness",
                table: "FactCheckResults",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "AnswerIsCorrect",
                table: "ExportRows",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageAnswer",
                table: "ExportRows",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageAnswerCorrectness",
                table: "ExportRows",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRelativeError",
                table: "ExportRows",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageSourceCorrectness",
                table: "ExportRows",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerIsCorrect",
                table: "FactCheckResults");

            migrationBuilder.DropColumn(
                name: "AverageAnswer",
                table: "FactCheckResults");

            migrationBuilder.DropColumn(
                name: "AverageAnswerCorrectness",
                table: "FactCheckResults");

            migrationBuilder.DropColumn(
                name: "AverageRelativeError",
                table: "FactCheckResults");

            migrationBuilder.DropColumn(
                name: "AverageSourceCorrectness",
                table: "FactCheckResults");

            migrationBuilder.DropColumn(
                name: "AnswerIsCorrect",
                table: "ExportRows");

            migrationBuilder.DropColumn(
                name: "AverageAnswer",
                table: "ExportRows");

            migrationBuilder.DropColumn(
                name: "AverageAnswerCorrectness",
                table: "ExportRows");

            migrationBuilder.DropColumn(
                name: "AverageRelativeError",
                table: "ExportRows");

            migrationBuilder.DropColumn(
                name: "AverageSourceCorrectness",
                table: "ExportRows");

            migrationBuilder.RenameColumn(
                name: "SourceIsCorrect",
                table: "FactCheckResults",
                newName: "IsCorrect");

            migrationBuilder.RenameColumn(
                name: "SourceIsCorrect",
                table: "ExportRows",
                newName: "IsCorrect");
        }
    }
}
