using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnovatEPAM.Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdeaCategoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Ideas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryData",
                table: "Ideas",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ideas_Category",
                table: "Ideas",
                column: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ideas_Category",
                table: "Ideas");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Ideas");

            migrationBuilder.DropColumn(
                name: "CategoryData",
                table: "Ideas");
        }
    }
}
