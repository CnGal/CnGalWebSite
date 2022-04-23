using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    public partial class AddImageNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Carousels",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "Carousels");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "fca03f69-8d4e-4f79-a4c2-fefec8b3ff91");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "abdc0232-336f-4e74-a2f0-bc69ff979ada");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "c7dbf939-6e7b-4297-81a8-741c5ba3df98");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e578",
                column: "ConcurrencyStamp",
                value: "1285f3ca-d917-4464-bd2e-2d4a90609efb");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "fb7366e9-723e-4e66-91ae-81cb4b57392b");
        }
    }
}
