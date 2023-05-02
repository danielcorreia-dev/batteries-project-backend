using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Migrations
{
    public partial class Renomear_ColunaScoresparaScore_daTabelaUserCompanyScores : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Scores",
                table: "UserCompanyScores",
                newName: "Score");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Title",
                table: "Companies",
                column: "Title",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_Title",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "UserCompanyScores",
                newName: "Scores");
        }
    }
}
