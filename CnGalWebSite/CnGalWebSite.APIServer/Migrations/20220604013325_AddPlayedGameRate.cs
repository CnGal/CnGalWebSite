using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    public partial class AddPlayedGameRate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditTime",
                table: "PlayedGames",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MusicSocre",
                table: "PlayedGames",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowPublicly",
                table: "PlayedGames",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TotalSocre",
                table: "PlayedGames",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "PlayedGameId",
                table: "Examines",
                type: "bigint",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "fa23d14d-7353-468f-9468-4467b20684a5");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "af6903fd-d259-44cc-bdd5-cc0e8f0e3753");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "352aaa90-3997-4015-97ca-5904a70225b3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e578",
                column: "ConcurrencyStamp",
                value: "ba59512b-64ef-4c56-956c-3bfec95f8e98");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "766b553c-edc8-4957-b111-36bb36763a8b");

            migrationBuilder.CreateIndex(
                name: "IX_Examines_PlayedGameId",
                table: "Examines",
                column: "PlayedGameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Examines_PlayedGames_PlayedGameId",
                table: "Examines",
                column: "PlayedGameId",
                principalTable: "PlayedGames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Examines_PlayedGames_PlayedGameId",
                table: "Examines");

            migrationBuilder.DropIndex(
                name: "IX_Examines_PlayedGameId",
                table: "Examines");

            migrationBuilder.DropColumn(
                name: "LastEditTime",
                table: "PlayedGames");

            migrationBuilder.DropColumn(
                name: "MusicSocre",
                table: "PlayedGames");

            migrationBuilder.DropColumn(
                name: "ShowPublicly",
                table: "PlayedGames");

            migrationBuilder.DropColumn(
                name: "TotalSocre",
                table: "PlayedGames");

            migrationBuilder.DropColumn(
                name: "PlayedGameId",
                table: "Examines");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "f04420e1-a543-4253-91c8-c79bcdea22ee");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "0c0dc2be-aab3-4385-aa28-dc2dd7144e95");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "272a1fc3-416e-44f5-a25e-8a72536b1006");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e578",
                column: "ConcurrencyStamp",
                value: "1c4de16a-57c7-4375-b551-91bd956550ab");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "776554e1-4527-4a8b-af4b-fb314df1aa68");
        }
    }
}
