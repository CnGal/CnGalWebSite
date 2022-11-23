using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.APIServer.Migrations
{
    public partial class AddVideos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "VideoId",
                table: "Outlink",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TagId",
                table: "FavoriteObjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "VideoId",
                table: "FavoriteObjects",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "VideoId",
                table: "Examines",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "VideoId",
                table: "EntryPicture",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "VideoId",
                table: "Comments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Videos",
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
                    Type = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Copyright = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    IsInteractive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsCreatedByCurrentUser = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastEditTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreateUserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReaderCount = table.Column<int>(type: "int", nullable: false),
                    CommentCount = table.Column<int>(type: "int", nullable: false),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanComment = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OriginalAuthor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PubishTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MainPage = table.Column<string>(type: "longtext", maxLength: 10000000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Videos_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArticleVideo",
                columns: table => new
                {
                    ArticlesId = table.Column<long>(type: "bigint", nullable: false),
                    VideosId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleVideo", x => new { x.ArticlesId, x.VideosId });
                    table.ForeignKey(
                        name: "FK_ArticleVideo_Articles_ArticlesId",
                        column: x => x.ArticlesId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArticleVideo_Videos_VideosId",
                        column: x => x.VideosId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EntryVideo",
                columns: table => new
                {
                    EntriesId = table.Column<int>(type: "int", nullable: false),
                    VideosId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryVideo", x => new { x.EntriesId, x.VideosId });
                    table.ForeignKey(
                        name: "FK_EntryVideo_Entries_EntriesId",
                        column: x => x.EntriesId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryVideo_Videos_VideosId",
                        column: x => x.VideosId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VideoRelation",
                columns: table => new
                {
                    VideoRelationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FromVideo = table.Column<long>(type: "bigint", nullable: true),
                    ToVideo = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoRelation", x => x.VideoRelationId);
                    table.ForeignKey(
                        name: "FK_VideoRelation_Video_From",
                        column: x => x.FromVideo,
                        principalTable: "Videos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VideoRelation_Video_To",
                        column: x => x.ToVideo,
                        principalTable: "Videos",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: null);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: null);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: null);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e578",
                column: "ConcurrencyStamp",
                value: null);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "Email", "LastOnlineTime", "NormalizedEmail", "RegistTime" },
                values: new object[] { "4f625bd7-ee93-4548-86b3-85b6d204f2c2", "123456789@qq.com", new DateTime(1, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "123456789@qq.com", new DateTime(2022, 11, 23, 13, 54, 33, 845, DateTimeKind.Utc).AddTicks(162) });

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
                name: "IX_Outlink_VideoId",
                table: "Outlink",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteObjects_TagId",
                table: "FavoriteObjects",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteObjects_VideoId",
                table: "FavoriteObjects",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Examines_VideoId",
                table: "Examines",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryPicture_VideoId",
                table: "EntryPicture",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_VideoId",
                table: "Comments",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleVideo_VideosId",
                table: "ArticleVideo",
                column: "VideosId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryVideo_VideosId",
                table: "EntryVideo",
                column: "VideosId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoRelation_FromVideo",
                table: "VideoRelation",
                column: "FromVideo");

            migrationBuilder.CreateIndex(
                name: "IX_VideoRelation_ToVideo",
                table: "VideoRelation",
                column: "ToVideo");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_CreateUserId",
                table: "Videos",
                column: "CreateUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Videos_VideoId",
                table: "Comments",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntryPicture_Videos_VideoId",
                table: "EntryPicture",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Examines_Videos_VideoId",
                table: "Examines",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteObjects_Tags_TagId",
                table: "FavoriteObjects",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteObjects_Videos_VideoId",
                table: "FavoriteObjects",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Outlink_Videos_VideoId",
                table: "Outlink",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Videos_VideoId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryPicture_Videos_VideoId",
                table: "EntryPicture");

            migrationBuilder.DropForeignKey(
                name: "FK_Examines_Videos_VideoId",
                table: "Examines");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteObjects_Tags_TagId",
                table: "FavoriteObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteObjects_Videos_VideoId",
                table: "FavoriteObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Outlink_Videos_VideoId",
                table: "Outlink");

            migrationBuilder.DropTable(
                name: "ArticleVideo");

            migrationBuilder.DropTable(
                name: "EntryVideo");

            migrationBuilder.DropTable(
                name: "VideoRelation");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropIndex(
                name: "IX_Outlink_VideoId",
                table: "Outlink");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteObjects_TagId",
                table: "FavoriteObjects");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteObjects_VideoId",
                table: "FavoriteObjects");

            migrationBuilder.DropIndex(
                name: "IX_Examines_VideoId",
                table: "Examines");

            migrationBuilder.DropIndex(
                name: "IX_EntryPicture_VideoId",
                table: "EntryPicture");

            migrationBuilder.DropIndex(
                name: "IX_Comments_VideoId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "Outlink");

            migrationBuilder.DropColumn(
                name: "TagId",
                table: "FavoriteObjects");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "FavoriteObjects");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "Examines");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "EntryPicture");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "Comments");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                column: "ConcurrencyStamp",
                value: "df9c6cd5-1faf-49eb-a0f3-b816c62cfc36");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e576",
                column: "ConcurrencyStamp",
                value: "b8ea63a8-dbd7-4534-85dd-b27b559f279c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e577",
                column: "ConcurrencyStamp",
                value: "6d965bdb-4478-47fb-a53d-7b1f160afacc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e578",
                column: "ConcurrencyStamp",
                value: "32af696d-f7f4-43e0-9831-65fd7b9d00ac");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "Email", "LastOnlineTime", "NormalizedEmail", "RegistTime" },
                values: new object[] { "074e891d-c841-471e-9e3f-5246fd9191e7", "1278490989@qq.com", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "1278490989@qq.com", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastEditTime",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastEditTime",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastEditTime",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastEditTime",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
