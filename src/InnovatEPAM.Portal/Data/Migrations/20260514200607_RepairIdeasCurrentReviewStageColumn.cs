using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnovatEPAM.Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class RepairIdeasCurrentReviewStageColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Veritabanı geçmişi ile şema uyumsuz kaldığında (sütun eksik, migration kaydı tam) güvenli onarım.
            migrationBuilder.Sql(@"ALTER TABLE ""Ideas"" ADD COLUMN IF NOT EXISTS ""CurrentReviewStage"" integer NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
