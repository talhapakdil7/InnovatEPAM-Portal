using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnovatEPAM.Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStageSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StageTransitions");

            migrationBuilder.DropColumn(
                name: "CurrentReviewStage",
                table: "Ideas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentReviewStage",
                table: "Ideas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StageTransitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdeaId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransitionedByAdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStage = table.Column<int>(type: "integer", nullable: true),
                    IsAdvance = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Outcome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RevertReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ToStage = table.Column<int>(type: "integer", nullable: false),
                    TransitionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageTransitions_Ideas_IdeaId",
                        column: x => x.IdeaId,
                        principalTable: "Ideas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StageTransitions_Users_TransitionedByAdminId",
                        column: x => x.TransitionedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StageTransitions_IdeaId",
                table: "StageTransitions",
                column: "IdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_StageTransitions_TransitionDate",
                table: "StageTransitions",
                column: "TransitionDate");

            migrationBuilder.CreateIndex(
                name: "IX_StageTransitions_TransitionedByAdminId",
                table: "StageTransitions",
                column: "TransitionedByAdminId");
        }
    }
}
