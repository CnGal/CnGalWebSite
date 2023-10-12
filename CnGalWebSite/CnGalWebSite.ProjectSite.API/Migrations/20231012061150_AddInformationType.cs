using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CnGalWebSite.ProjectSite.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInformationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsHidden",
                table: "StallInformationTypes",
                newName: "Hide");

            migrationBuilder.AddColumn<long>(
                name: "TypeId",
                table: "StallInformation",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_StallInformation_TypeId",
                table: "StallInformation",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_StallInformation_StallInformationTypes_TypeId",
                table: "StallInformation",
                column: "TypeId",
                principalTable: "StallInformationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StallInformation_StallInformationTypes_TypeId",
                table: "StallInformation");

            migrationBuilder.DropIndex(
                name: "IX_StallInformation_TypeId",
                table: "StallInformation");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "StallInformation");

            migrationBuilder.RenameColumn(
                name: "Hide",
                table: "StallInformationTypes",
                newName: "IsHidden");
        }
    }
}
