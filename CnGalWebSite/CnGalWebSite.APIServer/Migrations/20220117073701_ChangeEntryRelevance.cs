using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    public partial class ChangeEntryRelevance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Votes",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Disambigs",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArticleEntry",
                columns: table => new
                {
                    ArticlesId = table.Column<long>(type: "bigint", nullable: false),
                    EntriesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleEntry", x => new { x.ArticlesId, x.EntriesId });
                    table.ForeignKey(
                        name: "FK_ArticleEntry_Articles_ArticlesId",
                        column: x => x.ArticlesId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArticleEntry_Entries_EntriesId",
                        column: x => x.EntriesId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArticleRelations",
                columns: table => new
                {
                    ArticleRelationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FromArticle = table.Column<long>(type: "bigint", nullable: true),
                    ToArticle = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleRelations", x => x.ArticleRelationId);
                    table.ForeignKey(
                        name: "FK_ArticleRelation_Article_From",
                        column: x => x.FromArticle,
                        principalTable: "Articles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ArticleRelation_Entry_To",
                        column: x => x.ToArticle,
                        principalTable: "Articles",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EntryRelations",
                columns: table => new
                {
                    EntryRelationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FromEntry = table.Column<int>(type: "int", nullable: true),
                    ToEntry = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryRelations", x => x.EntryRelationId);
                    table.ForeignKey(
                        name: "FK_EntryRelation_Entry_From",
                        column: x => x.FromEntry,
                        principalTable: "Entries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EntryRelation_Entry_To",
                        column: x => x.ToEntry,
                        principalTable: "Entries",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Outlink",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BriefIntroduction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Link = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ArticleId = table.Column<long>(type: "bigint", nullable: true),
                    EntryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outlink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Outlink_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Outlink_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "b7c1a4b4-ab3a-44f9-9bce-52a30a2ef9f3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "1bf702fe-be00-4d68-829c-a94f33d8d61c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "22b186bb-456f-4327-8dd0-a5dc42826dc7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e578",
                column: "ConcurrencyStamp",
                value: "8427b962-3f1c-4fdc-a0ac-e64025679013");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "1c7489b3-aaf8-4e09-8380-a9e6ec0a829e", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_Votes_Name",
                table: "Votes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Disambigs_Name",
                table: "Disambigs",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleEntry_EntriesId",
                table: "ArticleEntry",
                column: "EntriesId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleRelations_FromArticle",
                table: "ArticleRelations",
                column: "FromArticle");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleRelations_ToArticle",
                table: "ArticleRelations",
                column: "ToArticle");

            migrationBuilder.CreateIndex(
                name: "IX_EntryRelations_FromEntry",
                table: "EntryRelations",
                column: "FromEntry");

            migrationBuilder.CreateIndex(
                name: "IX_EntryRelations_ToEntry",
                table: "EntryRelations",
                column: "ToEntry");

            migrationBuilder.CreateIndex(
                name: "IX_Outlink_ArticleId",
                table: "Outlink",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Outlink_EntryId",
                table: "Outlink",
                column: "EntryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleEntry");

            migrationBuilder.DropTable(
                name: "ArticleRelations");

            migrationBuilder.DropTable(
                name: "EntryRelations");

            migrationBuilder.DropTable(
                name: "Outlink");

            migrationBuilder.DropIndex(
                name: "IX_Votes_Name",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Disambigs_Name",
                table: "Disambigs");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Votes",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Disambigs",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "7cd1d3b4-e914-4c20-a1cd-950923934583");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "77c6af35-ca5c-43c7-93cc-3de4f9fee336");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "4de6d4b4-59c2-4b3f-820d-ee86201a8b57");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e578",
                column: "ConcurrencyStamp",
                value: "9e4ad5e2-8b7d-487e-975b-ec76b0d7c2c3");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "3fdf21e8-bdcc-4ff6-a693-5f505800ded1", new DateTime(2022, 1, 14, 18, 0, 26, 702, DateTimeKind.Unspecified).AddTicks(2868) });
        }
    }
}
