using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Active.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVoteVoterRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_votes_voters_VoterId",
                table: "votes");

            migrationBuilder.DropIndex(
                name: "IX_votes_VoterId",
                table: "votes");

            migrationBuilder.DropColumn(
                name: "VoterId",
                table: "votes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VoterId",
                table: "votes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_votes_VoterId",
                table: "votes",
                column: "VoterId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_votes_voters_VoterId",
                table: "votes",
                column: "VoterId",
                principalTable: "voters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
