using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    public partial class AddVote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "VoteId",
                table: "Comments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BriefIntroduction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MainPicture = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BackgroundPicture = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SmallBackgroundPicture = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Thumbnail = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MainPage = table.Column<string>(type: "longtext", maxLength: 10000000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastEditTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    BeginTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReaderCount = table.Column<int>(type: "int", nullable: false),
                    CommentCount = table.Column<int>(type: "int", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsAllowModification = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanComment = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    MinimumSelectionCount = table.Column<long>(type: "bigint", nullable: false),
                    MaximumSelectionCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArticleVote",
                columns: table => new
                {
                    ArticlesId = table.Column<long>(type: "bigint", nullable: false),
                    VotesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleVote", x => new { x.ArticlesId, x.VotesId });
                    table.ForeignKey(
                        name: "FK_ArticleVote_Articles_ArticlesId",
                        column: x => x.ArticlesId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArticleVote_Votes_VotesId",
                        column: x => x.VotesId,
                        principalTable: "Votes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EntryVote",
                columns: table => new
                {
                    EntriesId = table.Column<int>(type: "int", nullable: false),
                    VotesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryVote", x => new { x.EntriesId, x.VotesId });
                    table.ForeignKey(
                        name: "FK_EntryVote_Entries_EntriesId",
                        column: x => x.EntriesId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryVote_Votes_VotesId",
                        column: x => x.VotesId,
                        principalTable: "Votes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PeripheryVote",
                columns: table => new
                {
                    PeripheriesId = table.Column<long>(type: "bigint", nullable: false),
                    VotesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeripheryVote", x => new { x.PeripheriesId, x.VotesId });
                    table.ForeignKey(
                        name: "FK_PeripheryVote_Peripheries_PeripheriesId",
                        column: x => x.PeripheriesId,
                        principalTable: "Peripheries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeripheryVote_Votes_VotesId",
                        column: x => x.VotesId,
                        principalTable: "Votes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VoteOptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Text = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    EntryId = table.Column<int>(type: "int", nullable: true),
                    ArticleId = table.Column<long>(type: "bigint", nullable: true),
                    PeripheryId = table.Column<long>(type: "bigint", nullable: true),
                    VoteId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoteOptions_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoteOptions_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoteOptions_Peripheries_PeripheryId",
                        column: x => x.PeripheryId,
                        principalTable: "Peripheries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoteOptions_Votes_VoteId",
                        column: x => x.VoteId,
                        principalTable: "Votes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VoteUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ApplicationUserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VoteId = table.Column<long>(type: "bigint", nullable: false),
                    VotedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoteUsers_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoteUsers_Votes_VoteId",
                        column: x => x.VoteId,
                        principalTable: "Votes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VoteOptionVoteUser",
                columns: table => new
                {
                    SeletedOptionsId = table.Column<long>(type: "bigint", nullable: false),
                    VoteUsersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteOptionVoteUser", x => new { x.SeletedOptionsId, x.VoteUsersId });
                    table.ForeignKey(
                        name: "FK_VoteOptionVoteUser_VoteOptions_SeletedOptionsId",
                        column: x => x.SeletedOptionsId,
                        principalTable: "VoteOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoteOptionVoteUser_VoteUsers_VoteUsersId",
                        column: x => x.VoteUsersId,
                        principalTable: "VoteUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "c7b43029-07b2-4297-938e-56594856d290");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "ad7b9b6b-1b44-4a9b-89b4-08786da48907");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "073a6e45-2a0d-4035-a2eb-cf645713d0e3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e578",
                column: "ConcurrencyStamp",
                value: "8ebf273d-83f8-45fb-a1c3-e3564261e9b0");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "41d7e16a-2a7e-4b47-9c65-2212981a8156", new DateTime(2022, 1, 7, 21, 6, 22, 177, DateTimeKind.Unspecified).AddTicks(2218) });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_VoteId",
                table: "Comments",
                column: "VoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleVote_VotesId",
                table: "ArticleVote",
                column: "VotesId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryVote_VotesId",
                table: "EntryVote",
                column: "VotesId");

            migrationBuilder.CreateIndex(
                name: "IX_PeripheryVote_VotesId",
                table: "PeripheryVote",
                column: "VotesId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteOptions_ArticleId",
                table: "VoteOptions",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteOptions_EntryId",
                table: "VoteOptions",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteOptions_PeripheryId",
                table: "VoteOptions",
                column: "PeripheryId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteOptions_VoteId",
                table: "VoteOptions",
                column: "VoteId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteOptionVoteUser_VoteUsersId",
                table: "VoteOptionVoteUser",
                column: "VoteUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteUsers_ApplicationUserId",
                table: "VoteUsers",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteUsers_VoteId",
                table: "VoteUsers",
                column: "VoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Votes_VoteId",
                table: "Comments",
                column: "VoteId",
                principalTable: "Votes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Votes_VoteId",
                table: "Comments");

            migrationBuilder.DropTable(
                name: "ArticleVote");

            migrationBuilder.DropTable(
                name: "EntryVote");

            migrationBuilder.DropTable(
                name: "PeripheryVote");

            migrationBuilder.DropTable(
                name: "VoteOptionVoteUser");

            migrationBuilder.DropTable(
                name: "VoteOptions");

            migrationBuilder.DropTable(
                name: "VoteUsers");

            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Comments_VoteId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "VoteId",
                table: "Comments");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "a63a30c8-dd01-4c3e-a4b3-0775d031056a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "2d31910e-4572-4e69-b23c-260e10d87b8e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "e00b1d5c-ff23-4c9a-b5f8-a0f2f81eda36");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e578",
                column: "ConcurrencyStamp",
                value: "c8010ff9-cad5-449e-8a50-08bddd8401b6");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "RegistTime" },
                values: new object[] { "2f960a14-bed8-48af-bdd1-9525e1594f47", new DateTime(2021, 12, 25, 16, 55, 21, 170, DateTimeKind.Unspecified).AddTicks(7036) });
        }
    }
}
