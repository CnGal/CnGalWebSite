using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFavoriteFolderPublic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "FavoriteFolders",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditTime",
                table: "FavoriteFolders",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ReaderCount",
                table: "FavoriteFolders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowPublicly",
                table: "FavoriteFolders",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "FavoriteFolderId",
                table: "Examines",
                type: "bigint",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "Birthday", "ConcurrencyStamp", "LastChangePasswordTime", "RegistTime", "UnsealTime" },
                values: new object[] { null, "e4c75437-814e-434d-ba70-d5b14474aa0d", null, new DateTime(2022, 12, 10, 17, 1, 7, 509, DateTimeKind.Utc).AddTicks(6520), null });

            migrationBuilder.CreateIndex(
                name: "IX_Examines_FavoriteFolderId",
                table: "Examines",
                column: "FavoriteFolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Examines_FavoriteFolders_FavoriteFolderId",
                table: "Examines",
                column: "FavoriteFolderId",
                principalTable: "FavoriteFolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Examines_FavoriteFolders_FavoriteFolderId",
                table: "Examines");

            migrationBuilder.DropIndex(
                name: "IX_Examines_FavoriteFolderId",
                table: "Examines");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "FavoriteFolders");

            migrationBuilder.DropColumn(
                name: "LastEditTime",
                table: "FavoriteFolders");

            migrationBuilder.DropColumn(
                name: "ReaderCount",
                table: "FavoriteFolders");

            migrationBuilder.DropColumn(
                name: "ShowPublicly",
                table: "FavoriteFolders");

            migrationBuilder.DropColumn(
                name: "FavoriteFolderId",
                table: "Examines");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "Birthday", "ConcurrencyStamp", "LastChangePasswordTime", "RegistTime", "UnsealTime" },
                values: new object[] { null, "dc963bc0-7913-4d17-80c5-b82236443255", null, new DateTime(2022, 11, 30, 11, 21, 48, 43, DateTimeKind.Utc).AddTicks(8863), null });
        }
    }
}
