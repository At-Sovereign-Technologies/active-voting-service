using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Active.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "voting_terminals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "voting_tables",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "voting_places",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "voting_terminals");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "voting_tables");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "voting_places");
        }
    }
}
