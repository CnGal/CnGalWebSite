using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    public partial class AddBindQQ : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Range",
                table: "RobotReplies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "GroupQQ",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastChangePasswordTime",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "5aa1c1db-f14c-4738-a819-4211b1f5499a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "6b5bde12-4907-4c5a-83ef-87f9c64e633a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "8370c218-00ec-4d70-95d0-909743f622f9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e578",
                column: "ConcurrencyStamp",
                value: "98151686-508a-400a-a4a5-f4c5d2fe65d9");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "8e04cce8-f3d5-4348-beda-a10677a13df6");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Range",
                table: "RobotReplies");

            migrationBuilder.DropColumn(
                name: "GroupQQ",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastChangePasswordTime",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "2697b832-e9b4-4102-983c-2ea4dbd5e416");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "6952b593-30a8-43c6-96cc-242d22567f76");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "1bea4ee7-8bed-47dc-85d5-ea75886ce4c7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e578",
                column: "ConcurrencyStamp",
                value: "bcb5c64d-0732-43e3-93d1-7074ca7927ed");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "a4ca0379-e98f-4ab0-8924-72310b51b8b0");
        }
    }
}
