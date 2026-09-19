using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FairAI.Api.Migrations
{
    /// <inheritdoc />
    public partial class ic4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VoteCount",
                table: "coreset",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "X",
                table: "chatmessages",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Y",
                table: "chatmessages",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Z",
                table: "chatmessages",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoteCount",
                table: "coreset");

            migrationBuilder.DropColumn(
                name: "X",
                table: "chatmessages");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "chatmessages");

            migrationBuilder.DropColumn(
                name: "Z",
                table: "chatmessages");
        }
    }
}
