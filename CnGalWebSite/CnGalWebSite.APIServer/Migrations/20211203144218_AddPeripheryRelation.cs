using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    public partial class AddPeripheryRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Examines_Disambigs_DisambigId1",
                table: "Examines");

            migrationBuilder.DropIndex(
                name: "IX_Examines_DisambigId1",
                table: "Examines");

            migrationBuilder.DropColumn(
                name: "DisambigId1",
                table: "Examines");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Peripheries",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<bool>(
                name: "CanComment",
                table: "Peripheries",
                type: "tinyint(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Peripheries",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "DisambigId",
                table: "Examines",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "CanComment",
                table: "Entries",
                type: "tinyint(1)",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "CanComment",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "CanComment",
                table: "Articles",
                type: "tinyint(1)",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: true);

            migrationBuilder.CreateTable(
                name: "PeripheryRelations",
                columns: table => new
                {
                    PeripheryRelationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FromPeriphery = table.Column<long>(type: "bigint", nullable: true),
                    ToPeriphery = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeripheryRelations", x => x.PeripheryRelationId);
                    table.ForeignKey(
                        name: "FK_PeripheryRelation_Periphery_From",
                        column: x => x.FromPeriphery,
                        principalTable: "Peripheries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PeripheryRelation_Periphery_To",
                        column: x => x.ToPeriphery,
                        principalTable: "Peripheries",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "20993cd6-5a29-4501-897d-0d7449b7358b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "d2eba311-4087-4170-ac58-a4e67c20cab4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "6445ad78-ff45-417b-9374-9b80043e3caf");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "4881827f-d1a0-4c09-8b44-7554b5490ca8", new DateTime(2021, 12, 3, 22, 42, 17, 819, DateTimeKind.Unspecified).AddTicks(7569) });

            migrationBuilder.CreateIndex(
                name: "IX_Peripheries_Name",
                table: "Peripheries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Examines_DisambigId",
                table: "Examines",
                column: "DisambigId");

            migrationBuilder.CreateIndex(
                name: "IX_PeripheryRelations_FromPeriphery",
                table: "PeripheryRelations",
                column: "FromPeriphery");

            migrationBuilder.CreateIndex(
                name: "IX_PeripheryRelations_ToPeriphery",
                table: "PeripheryRelations",
                column: "ToPeriphery");

            migrationBuilder.AddForeignKey(
                name: "FK_Examines_Disambigs_DisambigId",
                table: "Examines",
                column: "DisambigId",
                principalTable: "Disambigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Examines_Disambigs_DisambigId",
                table: "Examines");

            migrationBuilder.DropTable(
                name: "PeripheryRelations");

            migrationBuilder.DropIndex(
                name: "IX_Peripheries_Name",
                table: "Peripheries");

            migrationBuilder.DropIndex(
                name: "IX_Examines_DisambigId",
                table: "Examines");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Peripheries");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Peripheries",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<bool>(
                name: "CanComment",
                table: "Peripheries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "DisambigId",
                table: "Examines",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisambigId1",
                table: "Examines",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "CanComment",
                table: "Entries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "CanComment",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "CanComment",
                table: "Articles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "6ce761a1-c24d-4888-b1a2-c4e7c0398c9e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "07872108-5149-4440-97ef-658e6d0bdda8");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "abf5c9d7-b996-4b67-8da7-ba4076a1fb43");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "5a72f656-d3bb-4eb1-900e-864f3acec77b", new DateTime(2021, 12, 3, 15, 9, 35, 347, DateTimeKind.Unspecified).AddTicks(5895) });

            migrationBuilder.CreateIndex(
                name: "IX_Examines_DisambigId1",
                table: "Examines",
                column: "DisambigId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Examines_Disambigs_DisambigId1",
                table: "Examines",
                column: "DisambigId1",
                principalTable: "Disambigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
