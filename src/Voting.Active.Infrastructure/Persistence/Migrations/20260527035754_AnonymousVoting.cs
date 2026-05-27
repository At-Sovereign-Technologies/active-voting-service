using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Active.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AnonymousVoting : Migration
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

            migrationBuilder.CreateTable(
                name: "jurors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Document = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ElectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jurors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_jurors_elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_jurors_ElectionId",
                table: "jurors",
                column: "ElectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "jurors");

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
