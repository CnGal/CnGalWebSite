using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    public partial class AddRoleBrithday : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCreatedByCurrentUser",
                table: "Articles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RoleBirthdays",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Birthday = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleBirthdays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleBirthdays_Entries_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "LastOnlineTime", "RegistTime" },
                values: new object[] { "dfe0a24b-25b1-49f6-8ba2-af37a3c0dbe4", new DateTime(1, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 11, 27, 16, 40, 3, 134, DateTimeKind.Utc).AddTicks(606) });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastEditTime",
                value: new DateTime(1, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastEditTime",
                value: new DateTime(1, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastEditTime",
                value: new DateTime(1, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastEditTime",
                value: new DateTime(1, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_RoleBirthdays_RoleId",
                table: "RoleBirthdays",
                column: "RoleId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleBirthdays");

            migrationBuilder.DropColumn(
                name: "IsCreatedByCurrentUser",
                table: "Articles");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "LastOnlineTime", "RegistTime" },
                values: new object[] { "4f625bd7-ee93-4548-86b3-85b6d204f2c2", new DateTime(1, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 11, 23, 13, 54, 33, 845, DateTimeKind.Utc).AddTicks(162) });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastEditTime",
                value: new DateTime(1, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastEditTime",
                value: new DateTime(1, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastEditTime",
                value: new DateTime(1, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastEditTime",
                value: new DateTime(1, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
