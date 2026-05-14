using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnovatEPAM.Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedByAdminId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Key);
                    table.ForeignKey(
                        name: "FK_SystemSettings_Users_LastModifiedByAdminId",
                        column: x => x.LastModifiedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Key", "LastModifiedByAdminId", "LastModifiedDate", "Value" },
                values: new object[] { "BlindReviewEnabled", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "false" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_LastModifiedByAdminId",
                table: "SystemSettings",
                column: "LastModifiedByAdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");
        }
    }
}
