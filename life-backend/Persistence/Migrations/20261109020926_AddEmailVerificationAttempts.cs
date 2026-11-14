using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Life.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailVerificationTokens_TokenHash",
                table: "EmailVerificationTokens");

            migrationBuilder.AddColumn<int>(
                name: "FailedAttempts",
                table: "EmailVerificationTokens",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedAttempts",
                table: "EmailVerificationTokens");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationTokens_TokenHash",
                table: "EmailVerificationTokens",
                column: "TokenHash",
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
