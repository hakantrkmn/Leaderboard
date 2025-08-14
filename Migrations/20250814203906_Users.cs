using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leaderboard.Migrations
{
    /// <inheritdoc />
    public partial class Users : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'utc'"),
                    PlayerLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    TrophyCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
