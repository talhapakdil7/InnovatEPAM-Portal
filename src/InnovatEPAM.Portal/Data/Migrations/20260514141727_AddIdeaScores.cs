using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnovatEPAM.Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdeaScores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IdeaScores",
                columns: table => new
                {
                    IdeaId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    Innovation = table.Column<int>(type: "integer", nullable: true),
                    TechnicalFeasibility = table.Column<int>(type: "integer", nullable: true),
                    BusinessImpact = table.Column<int>(type: "integer", nullable: true),
                    ImplementationValue = table.Column<int>(type: "integer", nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdeaScores", x => new { x.IdeaId, x.AdminId });
                    table.ForeignKey(
                        name: "FK_IdeaScores_Ideas_IdeaId",
                        column: x => x.IdeaId,
                        principalTable: "Ideas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IdeaScores_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdeaScores_AdminId",
                table: "IdeaScores",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_IdeaScores_IdeaId",
                table: "IdeaScores",
                column: "IdeaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdeaScores");
        }
    }
}
