using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FairAI.Api.Migrations
{
    /// <inheritdoc />
    public partial class modelfix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aistaterecords");

            migrationBuilder.DropTable(
                name: "chatmessages");

            migrationBuilder.DropTable(
                name: "corerecords");

            migrationBuilder.DropTable(
                name: "languageentries");

            migrationBuilder.DropTable(
                name: "neuronrecords");

            migrationBuilder.DropTable(
                name: "noderecords");

            migrationBuilder.DropTable(
                name: "chatsessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chatsessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chatsessions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "languageentries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DepthValue = table.Column<double>(type: "double", nullable: false),
                    HistoryValue = table.Column<double>(type: "double", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    Word = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_languageentries", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "aistaterecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DepthValue = table.Column<double>(type: "double", nullable: false),
                    HistoryValue = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aistaterecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_aistaterecords_chatsessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "chatsessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "chatmessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Role = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    X = table.Column<double>(type: "double", nullable: true),
                    Y = table.Column<double>(type: "double", nullable: true),
                    Z = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chatmessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chatmessages_chatsessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "chatsessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "corerecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Position = table.Column<double>(type: "double", nullable: false),
                    Range = table.Column<int>(type: "int", nullable: false),
                    Speed = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_corerecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_corerecords_chatsessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "chatsessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "neuronrecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Value = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_neuronrecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_neuronrecords_chatsessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "chatsessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "noderecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DepthValue = table.Column<double>(type: "double", nullable: false),
                    HistoryValue = table.Column<double>(type: "double", nullable: false),
                    MiddleValue = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_noderecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_noderecords_chatsessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "chatsessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_aistaterecords_SessionId",
                table: "aistaterecords",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_chatmessages_SessionId",
                table: "chatmessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_corerecords_SessionId",
                table: "corerecords",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_languageentries_Word",
                table: "languageentries",
                column: "Word",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_neuronrecords_SessionId",
                table: "neuronrecords",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_noderecords_SessionId",
                table: "noderecords",
                column: "SessionId");
        }
    }
}
