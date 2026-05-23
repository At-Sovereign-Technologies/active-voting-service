using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Active.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVotingCoreEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Document = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Party = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PhotoUrl = table.Column<string>(type: "text", nullable: false),
                    ElectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_candidates_elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "voters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Document = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HasVoted = table.Column<bool>(type: "boolean", nullable: false),
                    ElectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voters_elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "voting_terminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Secret = table.Column<string>(type: "text", nullable: false),
                    PublicKey = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    VotingTableId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voting_terminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voting_terminals_voting_tables_VotingTableId",
                        column: x => x.VotingTableId,
                        principalTable: "voting_tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "votes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VotingTerminalId = table.Column<Guid>(type: "uuid", nullable: false),
                    VotingTableId = table.Column<Guid>(type: "uuid", nullable: false),
                    VotingPlaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Signature = table.Column<string>(type: "text", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_votes_candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_votes_voters_VoterId",
                        column: x => x.VoterId,
                        principalTable: "voters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_votes_voting_places_VotingPlaceId",
                        column: x => x.VotingPlaceId,
                        principalTable: "voting_places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_votes_voting_tables_VotingTableId",
                        column: x => x.VotingTableId,
                        principalTable: "voting_tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_votes_voting_terminals_VotingTerminalId",
                        column: x => x.VotingTerminalId,
                        principalTable: "voting_terminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_candidates_ElectionId",
                table: "candidates",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_voters_Document",
                table: "voters",
                column: "Document",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voters_ElectionId",
                table: "voters",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_votes_CandidateId",
                table: "votes",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_votes_VoterId",
                table: "votes",
                column: "VoterId");

            migrationBuilder.CreateIndex(
                name: "IX_votes_VotingPlaceId",
                table: "votes",
                column: "VotingPlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_votes_VotingTableId",
                table: "votes",
                column: "VotingTableId");

            migrationBuilder.CreateIndex(
                name: "IX_votes_VotingTerminalId",
                table: "votes",
                column: "VotingTerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_voting_terminals_VotingTableId",
                table: "voting_terminals",
                column: "VotingTableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "votes");

            migrationBuilder.DropTable(
                name: "candidates");

            migrationBuilder.DropTable(
                name: "voters");

            migrationBuilder.DropTable(
                name: "voting_terminals");
        }
    }
}
