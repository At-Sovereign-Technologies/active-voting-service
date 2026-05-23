using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Active.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicateVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_votes_VoterId",
                table: "votes");

            migrationBuilder.CreateIndex(
                name: "IX_votes_VoterId",
                table: "votes",
                column: "VoterId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_votes_VoterId",
                table: "votes");

            migrationBuilder.CreateIndex(
                name: "IX_votes_VoterId",
                table: "votes",
                column: "VoterId");
        }
    }
}
