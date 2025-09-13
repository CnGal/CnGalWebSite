using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    /// <inheritdoc />
    public partial class AddExpoActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpoActivities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ForegroundImage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BackgroundImage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Hidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpoActivities", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ExpoTickets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActivityId = table.Column<long>(type: "bigint", nullable: false),
                    SeatInfo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nickname = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Number = table.Column<int>(type: "int", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Hidden = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpoTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpoTickets_ExpoActivities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "ExpoActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "f37b280d-e717-464a-842c-26eb0e5a40cf", new DateTime(2025, 9, 13, 22, 2, 20, 687, DateTimeKind.Utc).AddTicks(576) });

            migrationBuilder.CreateIndex(
                name: "IX_ExpoActivities_Name",
                table: "ExpoActivities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpoTickets_ActivityId",
                table: "ExpoTickets",
                column: "ActivityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpoTickets");

            migrationBuilder.DropTable(
                name: "ExpoActivities");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "d3963102-8d1e-4da7-9705-6fa628c5dd04", new DateTime(2025, 7, 13, 15, 53, 49, 510, DateTimeKind.Utc).AddTicks(4706) });
        }
    }
}
