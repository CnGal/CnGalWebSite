using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace CnGalWebSite.APIServer.Migrations
{
    public partial class AddPerfection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PerfectionId",
                table: "Entries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Perfections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Grade = table.Column<double>(type: "double", nullable: false),
                    VictoryPercentage = table.Column<double>(type: "double", nullable: false),
                    EntryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Perfections_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PerfectionChecks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Grade = table.Column<double>(type: "double", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Infor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CheckType = table.Column<int>(type: "int", nullable: false),
                    DefectType = table.Column<int>(type: "int", nullable: false),
                    PerfectionId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfectionChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfectionChecks_Perfections_PerfectionId",
                        column: x => x.PerfectionId,
                        principalTable: "Perfections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "01409a17-1831-49df-ba8a-4855e31b7ea0");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "97513fd8-2aea-4820-9e55-2e5d892b7315");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "91c8b71c-6986-44dc-b179-55efb35849cd");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "273bcedf-e4ec-4863-8951-63b5d1f2f71b", new DateTime(2021, 10, 10, 20, 57, 8, 584, DateTimeKind.Unspecified).AddTicks(9161) });

            migrationBuilder.CreateIndex(
                name: "IX_PerfectionChecks_PerfectionId",
                table: "PerfectionChecks",
                column: "PerfectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Perfections_EntryId",
                table: "Perfections",
                column: "EntryId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerfectionChecks");

            migrationBuilder.DropTable(
                name: "Perfections");

            migrationBuilder.DropColumn(
                name: "PerfectionId",
                table: "Entries");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "a95ec0dd-8a7c-47a2-becc-cdbfb7cd5cbd");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "e9ccb79a-4441-4a6a-8f9b-38787434e820");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "89c6f2ec-38ac-4123-936e-bcc0ea8fe375");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "cb0122ab-d9de-421e-8b74-bf6ee0499ad3", new DateTime(2021, 10, 9, 19, 41, 59, 659, DateTimeKind.Unspecified).AddTicks(8244) });
        }
    }
}
