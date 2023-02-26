using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingAndEntryWebsite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Template",
                table: "Entries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsNeedNotification = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Open = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LotteryId = table.Column<int>(type: "int", nullable: false),
                    BookingCount = table.Column<int>(type: "int", nullable: false),
                    EntryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EntryWebsites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Introduction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Html = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryWebsites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryWebsites_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BookingGoal",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Target = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingGoal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingGoal_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BookingUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsNotified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BookingTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BookingId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingUsers_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingUsers_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EntryWebsiteImage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CarouseWebsitelId = table.Column<long>(type: "bigint", nullable: false),
                    CarouselWebsiteId = table.Column<long>(type: "bigint", nullable: true),
                    BackgroundImageWebsiteId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryWebsiteImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryWebsiteImage_EntryWebsites_BackgroundImageWebsiteId",
                        column: x => x.BackgroundImageWebsiteId,
                        principalTable: "EntryWebsites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryWebsiteImage_EntryWebsites_CarouselWebsiteId",
                        column: x => x.CarouselWebsiteId,
                        principalTable: "EntryWebsites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "Birthday", "ConcurrencyStamp", "LastChangePasswordTime", "RegistTime", "UnsealTime" },
                values: new object[] { null, "1ccc753b-ae39-4568-a808-53cfed276b00", null, new DateTime(2023, 2, 26, 12, 13, 25, 129, DateTimeKind.Utc).AddTicks(3329), null });

            migrationBuilder.CreateIndex(
                name: "IX_BookingGoal_BookingId",
                table: "BookingGoal",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EntryId",
                table: "Bookings",
                column: "EntryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingUsers_ApplicationUserId",
                table: "BookingUsers",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingUsers_BookingId",
                table: "BookingUsers",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryWebsiteImage_BackgroundImageWebsiteId",
                table: "EntryWebsiteImage",
                column: "BackgroundImageWebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryWebsiteImage_CarouselWebsiteId",
                table: "EntryWebsiteImage",
                column: "CarouselWebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryWebsites_EntryId",
                table: "EntryWebsites",
                column: "EntryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingGoal");

            migrationBuilder.DropTable(
                name: "BookingUsers");

            migrationBuilder.DropTable(
                name: "EntryWebsiteImage");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "EntryWebsites");

            migrationBuilder.DropColumn(
                name: "Template",
                table: "Entries");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "Birthday", "ConcurrencyStamp", "LastChangePasswordTime", "RegistTime", "UnsealTime" },
                values: new object[] { null, "7f7ccc1b-5e50-418d-ac95-200e8c7df1ea", null, new DateTime(2023, 2, 20, 12, 57, 45, 825, DateTimeKind.Utc).AddTicks(4569), null });
        }
    }
}
