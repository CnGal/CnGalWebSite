using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSteamAppReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SteamAppRreviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    recommendationid = table.Column<int>(type: "int", nullable: false),
                    steamid = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    num_games_owned = table.Column<long>(type: "bigint", nullable: true),
                    num_reviews = table.Column<long>(type: "bigint", nullable: true),
                    playtime_forever = table.Column<long>(type: "bigint", nullable: true),
                    playtime_last_two_weeks = table.Column<long>(type: "bigint", nullable: true),
                    playtime_at_review = table.Column<long>(type: "bigint", nullable: true),
                    last_played = table.Column<long>(type: "bigint", nullable: true),
                    language = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    review = table.Column<string>(type: "longtext", maxLength: 10000000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    timestamp_created = table.Column<long>(type: "bigint", nullable: true),
                    timestamp_updated = table.Column<long>(type: "bigint", nullable: true),
                    voted_up = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    votes_up = table.Column<long>(type: "bigint", nullable: true),
                    votes_funny = table.Column<long>(type: "bigint", nullable: true),
                    weighted_vote_score = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    comment_count = table.Column<long>(type: "bigint", nullable: true),
                    steam_purchase = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    received_for_free = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    written_during_early_access = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    hidden_in_steam_china = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    steam_china_location = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    developer_response = table.Column<string>(type: "longtext", maxLength: 10000000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    timestamp_dev_responded = table.Column<long>(type: "bigint", nullable: true),
                    appid = table.Column<long>(type: "bigint", nullable: false),
                    update_time = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamAppRreviews", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "a585f875-e7b8-4926-9b22-847b089470be", new DateTime(2023, 11, 23, 23, 47, 56, 212, DateTimeKind.Utc).AddTicks(6861) });

            migrationBuilder.CreateIndex(
                name: "IX_SteamAppRreviews_recommendationid",
                table: "SteamAppRreviews",
                column: "recommendationid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SteamAppRreviews");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "e53296ab-f9ef-4d26-895b-630cf903dcd8", new DateTime(2023, 11, 19, 13, 20, 49, 287, DateTimeKind.Utc).AddTicks(1725) });
        }
    }
}
