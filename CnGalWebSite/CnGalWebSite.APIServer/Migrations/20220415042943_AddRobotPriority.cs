using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    public partial class AddRobotPriority : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "RobotReplies",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "RobotReplies");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "b6f9f614-87e2-4b32-85ad-f9c33cc400f3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "d8747012-6573-4792-8a1d-219769b0e78c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "f738cd7e-ecee-44ae-a2e4-6bb28df3e2ea");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e578",
                column: "ConcurrencyStamp",
                value: "c7d9a8fe-19de-4967-9805-ad00eeed71a9");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "d570a73b-84db-4d5b-a5aa-7c9138427050");
        }
    }
}
