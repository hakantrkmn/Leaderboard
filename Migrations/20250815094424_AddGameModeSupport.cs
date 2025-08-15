using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leaderboard.Migrations
{
    /// <inheritdoc />
    public partial class AddGameModeSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Leaderboard",
                table: "Leaderboard");

            migrationBuilder.DropIndex(
                name: "IX_Leaderboard_Ranking",
                table: "Leaderboard");

            migrationBuilder.AddColumn<int>(
                name: "GameMode",
                table: "Leaderboard",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Leaderboard",
                table: "Leaderboard",
                columns: new[] { "UserId", "GameMode" });

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboard_GameMode_Ranking",
                table: "Leaderboard",
                columns: new[] { "GameMode", "Score", "RegistrationDateUtc", "PlayerLevel", "TrophyCount" });

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboard_User_GameMode",
                table: "Leaderboard",
                columns: new[] { "UserId", "GameMode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Leaderboard",
                table: "Leaderboard");

            migrationBuilder.DropIndex(
                name: "IX_Leaderboard_GameMode_Ranking",
                table: "Leaderboard");

            migrationBuilder.DropIndex(
                name: "IX_Leaderboard_User_GameMode",
                table: "Leaderboard");

            migrationBuilder.DropColumn(
                name: "GameMode",
                table: "Leaderboard");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Leaderboard",
                table: "Leaderboard",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboard_Ranking",
                table: "Leaderboard",
                columns: new[] { "Score", "RegistrationDateUtc", "PlayerLevel", "TrophyCount" });
        }
    }
}
