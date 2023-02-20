using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayTimeAndOwners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstimationOwners",
                table: "SteamInforTableModels",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "PlayTime",
                table: "SteamInforTableModels",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "EstimationOwnersMax",
                table: "SteamInfors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstimationOwnersMin",
                table: "SteamInfors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "Birthday", "ConcurrencyStamp", "LastChangePasswordTime", "RegistTime", "UnsealTime" },
                values: new object[] { null, "7f7ccc1b-5e50-418d-ac95-200e8c7df1ea", null, new DateTime(2023, 2, 20, 12, 57, 45, 825, DateTimeKind.Utc).AddTicks(4569), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimationOwners",
                table: "SteamInforTableModels");

            migrationBuilder.DropColumn(
                name: "PlayTime",
                table: "SteamInforTableModels");

            migrationBuilder.DropColumn(
                name: "EstimationOwnersMax",
                table: "SteamInfors");

            migrationBuilder.DropColumn(
                name: "EstimationOwnersMin",
                table: "SteamInfors");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "Birthday", "ConcurrencyStamp", "LastChangePasswordTime", "RegistTime", "UnsealTime" },
                values: new object[] { null, "e4c75437-814e-434d-ba70-d5b14474aa0d", null, new DateTime(2022, 12, 10, 17, 1, 7, 509, DateTimeKind.Utc).AddTicks(6520), null });
        }
    }
}
