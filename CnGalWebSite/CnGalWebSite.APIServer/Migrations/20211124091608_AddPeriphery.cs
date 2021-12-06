using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace CnGalWebSite.APIServer.Migrations
{
    public partial class AddPeriphery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PeripheryId",
                table: "FavoriteObjects",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PeripheryId",
                table: "Examines",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PeripheryId",
                table: "EntryPicture",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PeripheryId",
                table: "Comments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Peripheries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BriefIntroduction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MainPicture = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BackgroundPicture = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SmallBackgroundPicture = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Thumbnail = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Author = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Material = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    LastEditTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReaderCount = table.Column<int>(type: "int", nullable: false),
                    CollectedCount = table.Column<int>(type: "int", nullable: false),
                    CommentCount = table.Column<int>(type: "int", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanComment = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Peripheries", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PeripheryRelevanceEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PeripheryId = table.Column<long>(type: "bigint", nullable: true),
                    EntryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeripheryRelevanceEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeripheryRelevanceEntries_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeripheryRelevanceEntries_Peripheries_PeripheryId",
                        column: x => x.PeripheryId,
                        principalTable: "Peripheries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PeripheryRelevanceUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PeripheryId = table.Column<long>(type: "bigint", nullable: false),
                    StartOwnedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeripheryRelevanceUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeripheryRelevanceUsers_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeripheryRelevanceUsers_Peripheries_PeripheryId",
                        column: x => x.PeripheryId,
                        principalTable: "Peripheries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "03d597c8-9f77-499f-8763-77a41f2fff33");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "4d74728b-2891-416b-9300-74ed9764db86");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "9e209453-f6f5-4509-93b9-65ba76a82c2f");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "71ad3827-142f-4d7c-a3b2-c8224491863d", new DateTime(2021, 11, 24, 17, 16, 7, 890, DateTimeKind.Unspecified).AddTicks(2752) });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteObjects_PeripheryId",
                table: "FavoriteObjects",
                column: "PeripheryId");

            migrationBuilder.CreateIndex(
                name: "IX_Examines_PeripheryId",
                table: "Examines",
                column: "PeripheryId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryPicture_PeripheryId",
                table: "EntryPicture",
                column: "PeripheryId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PeripheryId",
                table: "Comments",
                column: "PeripheryId");

            migrationBuilder.CreateIndex(
                name: "IX_PeripheryRelevanceEntries_EntryId",
                table: "PeripheryRelevanceEntries",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PeripheryRelevanceEntries_PeripheryId",
                table: "PeripheryRelevanceEntries",
                column: "PeripheryId");

            migrationBuilder.CreateIndex(
                name: "IX_PeripheryRelevanceUsers_ApplicationUserId",
                table: "PeripheryRelevanceUsers",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PeripheryRelevanceUsers_PeripheryId",
                table: "PeripheryRelevanceUsers",
                column: "PeripheryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Peripheries_PeripheryId",
                table: "Comments",
                column: "PeripheryId",
                principalTable: "Peripheries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntryPicture_Peripheries_PeripheryId",
                table: "EntryPicture",
                column: "PeripheryId",
                principalTable: "Peripheries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Examines_Peripheries_PeripheryId",
                table: "Examines",
                column: "PeripheryId",
                principalTable: "Peripheries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteObjects_Peripheries_PeripheryId",
                table: "FavoriteObjects",
                column: "PeripheryId",
                principalTable: "Peripheries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Peripheries_PeripheryId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryPicture_Peripheries_PeripheryId",
                table: "EntryPicture");

            migrationBuilder.DropForeignKey(
                name: "FK_Examines_Peripheries_PeripheryId",
                table: "Examines");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteObjects_Peripheries_PeripheryId",
                table: "FavoriteObjects");

            migrationBuilder.DropTable(
                name: "PeripheryRelevanceEntries");

            migrationBuilder.DropTable(
                name: "PeripheryRelevanceUsers");

            migrationBuilder.DropTable(
                name: "Peripheries");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteObjects_PeripheryId",
                table: "FavoriteObjects");

            migrationBuilder.DropIndex(
                name: "IX_Examines_PeripheryId",
                table: "Examines");

            migrationBuilder.DropIndex(
                name: "IX_EntryPicture_PeripheryId",
                table: "EntryPicture");

            migrationBuilder.DropIndex(
                name: "IX_Comments_PeripheryId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "PeripheryId",
                table: "FavoriteObjects");

            migrationBuilder.DropColumn(
                name: "PeripheryId",
                table: "Examines");

            migrationBuilder.DropColumn(
                name: "PeripheryId",
                table: "EntryPicture");

            migrationBuilder.DropColumn(
                name: "PeripheryId",
                table: "Comments");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "0f676044-a679-4e92-9861-ce4a9586bede");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "f9346d40-8c1c-4bb7-aee7-637fdaaed5fd");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "3304c9f9-65da-47ee-9eed-66d581cfc166");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "a207fe4e-ce4a-42cd-827c-538f19129fe6", new DateTime(2021, 10, 13, 20, 2, 15, 366, DateTimeKind.Unspecified).AddTicks(9536) });
        }
    }
}
