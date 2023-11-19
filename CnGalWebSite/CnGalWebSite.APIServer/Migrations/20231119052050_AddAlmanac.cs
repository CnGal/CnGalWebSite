using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAlmanac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Almanacs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BriefIntroduction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Almanacs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AlmanacArticles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ArticleId = table.Column<long>(type: "bigint", nullable: false),
                    AlmanacId = table.Column<long>(type: "bigint", nullable: false),
                    Image = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Hide = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlmanacArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlmanacArticles_Almanacs_AlmanacId",
                        column: x => x.AlmanacId,
                        principalTable: "Almanacs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlmanacArticles_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AlmanacEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EntryId = table.Column<int>(type: "int", nullable: false),
                    AlmanacId = table.Column<long>(type: "bigint", nullable: false),
                    Image = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Hide = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlmanacEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlmanacEntries_Almanacs_AlmanacId",
                        column: x => x.AlmanacId,
                        principalTable: "Almanacs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlmanacEntries_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "e53296ab-f9ef-4d26-895b-630cf903dcd8", new DateTime(2023, 11, 19, 13, 20, 49, 287, DateTimeKind.Utc).AddTicks(1725) });

            migrationBuilder.CreateIndex(
                name: "IX_AlmanacArticles_AlmanacId",
                table: "AlmanacArticles",
                column: "AlmanacId");

            migrationBuilder.CreateIndex(
                name: "IX_AlmanacArticles_ArticleId",
                table: "AlmanacArticles",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_AlmanacEntries_AlmanacId",
                table: "AlmanacEntries",
                column: "AlmanacId");

            migrationBuilder.CreateIndex(
                name: "IX_AlmanacEntries_EntryId",
                table: "AlmanacEntries",
                column: "EntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlmanacArticles");

            migrationBuilder.DropTable(
                name: "AlmanacEntries");

            migrationBuilder.DropTable(
                name: "Almanacs");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "00dfc3c2-8d7e-4938-ad11-6b3de1305bf5", new DateTime(2023, 9, 28, 20, 46, 4, 578, DateTimeKind.Utc).AddTicks(5257) });
        }
    }
}
