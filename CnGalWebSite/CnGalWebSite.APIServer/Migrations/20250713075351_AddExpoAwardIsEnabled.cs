using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    /// <inheritdoc />
    public partial class AddExpoAwardIsEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "ExpoAwards",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "d3963102-8d1e-4da7-9705-6fa628c5dd04", new DateTime(2025, 7, 13, 15, 53, 49, 510, DateTimeKind.Utc).AddTicks(4706) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "ExpoAwards");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "b47d8bbb-d7c8-4949-8c64-e0a8dc2da03f", new DateTime(2025, 7, 4, 22, 24, 10, 853, DateTimeKind.Utc).AddTicks(1935) });
        }
    }
}
