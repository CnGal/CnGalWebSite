using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace CnGalWebSite.APIServer.Migrations
{
    public partial class AddPeripheryInfor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Peripheries",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "IndividualParts",
                table: "Peripheries",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableItem",
                table: "Peripheries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReprint",
                table: "Peripheries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PageCount",
                table: "Peripheries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Price",
                table: "Peripheries",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Peripheries",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "SongCount",
                table: "Peripheries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Peripheries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "e400a221-f3b5-4d2d-8ba2-e6835d874e6e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "6385e2d8-5d61-4d6d-a750-fc7c2ca197cd");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "898615e1-826c-43b3-81b7-877bd317c095");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "2ce2292e-ec2b-4b63-954f-55a456395003", new DateTime(2021, 11, 30, 16, 48, 51, 316, DateTimeKind.Unspecified).AddTicks(1432) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Peripheries");

            migrationBuilder.DropColumn(
                name: "IndividualParts",
                table: "Peripheries");

            migrationBuilder.DropColumn(
                name: "IsAvailableItem",
                table: "Peripheries");

            migrationBuilder.DropColumn(
                name: "IsReprint",
                table: "Peripheries");

            migrationBuilder.DropColumn(
                name: "PageCount",
                table: "Peripheries");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Peripheries");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Peripheries");

            migrationBuilder.DropColumn(
                name: "SongCount",
                table: "Peripheries");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Peripheries");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "27477bc3-d43f-4175-a9db-aafdaa0db435");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "6e5bc83a-d7a6-478d-9d37-c664f1fd59d1");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "772802cb-c20c-468d-a40f-16809f01a16e");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "c299656a-4514-4e20-9c8d-fcf7ccb735f2", new DateTime(2021, 11, 27, 9, 47, 39, 254, DateTimeKind.Unspecified).AddTicks(4396) });
        }
    }
}
