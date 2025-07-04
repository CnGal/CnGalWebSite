using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    /// <inheritdoc />
    public partial class AddOtherOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OtherOptionTexts",
                table: "QuestionResponses",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsOtherOption",
                table: "QuestionOptions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "b47d8bbb-d7c8-4949-8c64-e0a8dc2da03f", new DateTime(2025, 7, 4, 22, 24, 10, 853, DateTimeKind.Utc).AddTicks(1935) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherOptionTexts",
                table: "QuestionResponses");

            migrationBuilder.DropColumn(
                name: "IsOtherOption",
                table: "QuestionOptions");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "97606b44-26ee-4ef9-82d3-5720243c5127", new DateTime(2025, 7, 2, 21, 35, 21, 287, DateTimeKind.Utc).AddTicks(2359) });
        }
    }
}
