using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Active.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VotingTableId1",
                table: "voting_terminals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VotingPlaceId1",
                table: "voting_tables",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_voting_terminals_VotingTableId1",
                table: "voting_terminals",
                column: "VotingTableId1");

            migrationBuilder.CreateIndex(
                name: "IX_voting_tables_VotingPlaceId1",
                table: "voting_tables",
                column: "VotingPlaceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_voting_tables_voting_places_VotingPlaceId1",
                table: "voting_tables",
                column: "VotingPlaceId1",
                principalTable: "voting_places",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_voting_terminals_voting_tables_VotingTableId1",
                table: "voting_terminals",
                column: "VotingTableId1",
                principalTable: "voting_tables",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_voting_tables_voting_places_VotingPlaceId1",
                table: "voting_tables");

            migrationBuilder.DropForeignKey(
                name: "FK_voting_terminals_voting_tables_VotingTableId1",
                table: "voting_terminals");

            migrationBuilder.DropIndex(
                name: "IX_voting_terminals_VotingTableId1",
                table: "voting_terminals");

            migrationBuilder.DropIndex(
                name: "IX_voting_tables_VotingPlaceId1",
                table: "voting_tables");

            migrationBuilder.DropColumn(
                name: "VotingTableId1",
                table: "voting_terminals");

            migrationBuilder.DropColumn(
                name: "VotingPlaceId1",
                table: "voting_tables");
        }
    }
}
