using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    /// <inheritdoc />
    public partial class AddExpo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpoGames",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Hidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpoGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpoGames_Entries_GameId",
                        column: x => x.GameId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ExpoTags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Hidden = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpoTags", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ExpoGameExpoTag",
                columns: table => new
                {
                    GamesId = table.Column<long>(type: "bigint", nullable: false),
                    TagsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpoGameExpoTag", x => new { x.GamesId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ExpoGameExpoTag_ExpoGames_GamesId",
                        column: x => x.GamesId,
                        principalTable: "ExpoGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpoGameExpoTag_ExpoTags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "ExpoTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "087193d7-cfe3-4703-ae50-c4de2902a97c", new DateTime(2025, 5, 2, 16, 6, 32, 436, DateTimeKind.Utc).AddTicks(2648) });

            migrationBuilder.CreateIndex(
                name: "IX_ExpoGameExpoTag_TagsId",
                table: "ExpoGameExpoTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpoGames_GameId",
                table: "ExpoGames",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpoGameExpoTag");

            migrationBuilder.DropTable(
                name: "ExpoGames");

            migrationBuilder.DropTable(
                name: "ExpoTags");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "aa6c10ba-9997-446b-ab58-26b22369c056", new DateTime(2024, 8, 18, 14, 11, 34, 846, DateTimeKind.Utc).AddTicks(40) });
        }
    }
}
