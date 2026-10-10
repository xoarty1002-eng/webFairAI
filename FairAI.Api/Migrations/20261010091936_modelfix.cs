using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FairAI.Api.Migrations
{
    /// <inheritdoc />
    public partial class modelfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Word",
                table: "dataset",
                newName: "LastWord");

            migrationBuilder.RenameColumn(
                name: "HistoryValue",
                table: "dataset",
                newName: "MeaningValue");

            migrationBuilder.RenameColumn(
                name: "DepthValue",
                table: "dataset",
                newName: "LanguageValue");

            migrationBuilder.AddColumn<string>(
                name: "FirstWord",
                table: "dataset",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstWord",
                table: "dataset");

            migrationBuilder.RenameColumn(
                name: "MeaningValue",
                table: "dataset",
                newName: "HistoryValue");

            migrationBuilder.RenameColumn(
                name: "LastWord",
                table: "dataset",
                newName: "Word");

            migrationBuilder.RenameColumn(
                name: "LanguageValue",
                table: "dataset",
                newName: "DepthValue");
        }
    }
}
