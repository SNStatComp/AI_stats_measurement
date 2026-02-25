using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_stats_measurement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_modelResponses",
                table: "modelResponses");

            migrationBuilder.RenameTable(
                name: "modelResponses",
                newName: "ModelResponses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModelResponses",
                table: "ModelResponses",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ModelResponses",
                table: "ModelResponses");

            migrationBuilder.RenameTable(
                name: "ModelResponses",
                newName: "modelResponses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_modelResponses",
                table: "modelResponses",
                column: "Id");
        }
    }
}
