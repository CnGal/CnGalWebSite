using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoreInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlatformType = table.Column<int>(type: "int", nullable: false),
                    PlatformName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Link = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    State = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<int>(type: "int", nullable: false),
                    UpdateType = table.Column<int>(type: "int", nullable: false),
                    OriginalPrice = table.Column<double>(type: "double", nullable: true),
                    PriceNow = table.Column<double>(type: "double", nullable: true),
                    CutNow = table.Column<double>(type: "double", nullable: true),
                    PriceLowest = table.Column<double>(type: "double", nullable: true),
                    CutLowest = table.Column<double>(type: "double", nullable: true),
                    PlayTime = table.Column<int>(type: "int", nullable: true),
                    EvaluationCount = table.Column<int>(type: "int", nullable: true),
                    RecommendationRate = table.Column<double>(type: "double", nullable: true),
                    EstimationOwnersMax = table.Column<int>(type: "int", nullable: true),
                    EstimationOwnersMin = table.Column<int>(type: "int", nullable: true),
                    EntryId = table.Column<int>(type: "int", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreInfo_Entries_EntryId",
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
                columns: new[] { "Birthday", "ConcurrencyStamp", "LastChangePasswordTime", "RegistTime", "UnsealTime" },
                values: new object[] { null, "4bc65de1-ba15-4f6b-8597-01b286c25823", null, new DateTime(2023, 5, 14, 15, 57, 27, 339, DateTimeKind.Utc).AddTicks(3207), null });

            migrationBuilder.CreateIndex(
                name: "IX_StoreInfo_EntryId",
                table: "StoreInfo",
                column: "EntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoreInfo");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "Birthday", "ConcurrencyStamp", "LastChangePasswordTime", "RegistTime", "UnsealTime" },
                values: new object[] { null, "8e44d604-3b07-43af-9171-349c25809c26", null, new DateTime(2023, 4, 19, 19, 32, 48, 661, DateTimeKind.Utc).AddTicks(6492), null });
        }
    }
}
