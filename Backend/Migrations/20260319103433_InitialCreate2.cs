using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_stats_measurement.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SquareMeanRootError",
                table: "FactCheckResults",
                newName: "AbsoluteError");

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Prompts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Prompts");

            migrationBuilder.RenameColumn(
                name: "AbsoluteError",
                table: "FactCheckResults",
                newName: "SquareMeanRootError");
        }
    }
}
