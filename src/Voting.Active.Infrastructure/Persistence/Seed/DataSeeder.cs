using Microsoft.EntityFrameworkCore;
using Voting.Active.Domain.Entities;
using Voting.Active.Domain.Enums;

namespace Voting.Active.Infrastructure.Persistence.Seed;

public static class DataSeeder
{
    private const string DemoTerminalPublicKey = "ac5eff87b69c511c6fbae77e127de867aca6756711ce15f0c8df0f42e911b14a";

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Elections.AnyAsync())
        {
            return;
        }

        var election = new Election
        {
            Id = Guid.NewGuid(),
            Name = "Presidenciales 2026",
            ElectionType = "presidential",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
        };

        var votingPlace = new VotingPlace
        {
            Id = Guid.NewGuid(),
            Name = "Puesto Central Bogota",
            Latitude = 4.7110,
            Longitude = -74.0721,
            Status = OperationalStatus.Active
        };

        var tables = new List<VotingTable>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Code = "MESA-001",
                VotingPlaceId = votingPlace.Id,
                Status = OperationalStatus.Active
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "MESA-002",
                VotingPlaceId = votingPlace.Id,
                Status = OperationalStatus.Active
            }
        };

        var terminals = new List<VotingTerminal>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Secret = "demo-terminal-1-secret",
                PublicKey = DemoTerminalPublicKey,
                VotingTableId = tables[0].Id,
                Status = OperationalStatus.Active
            },
            new()
            {
                Id = Guid.NewGuid(),
                Secret = "demo-terminal-2-secret",
                PublicKey = DemoTerminalPublicKey,
                VotingTableId = tables[1].Id,
                Status = OperationalStatus.Active
            }
        };

        var candidate1 = new Candidate
        {
            Id = Guid.NewGuid(),
            Name = "Carlos Perez",
            Document = "123456789",
            Party = "Partido Azul",
            PhotoUrl = "https://photo.test/candidate1.png",
            ElectionId = election.Id
        };

        var candidate2 = new Candidate
        {
            Id = Guid.NewGuid(),
            Name = "Laura Martinez",
            Document = "987654321",
            Party = "Partido Verde",
            PhotoUrl = "https://photo.test/candidate2.png",
            ElectionId = election.Id
        };

        var candidate3 = new Candidate
        {
            Id = Guid.NewGuid(),
            Name = "Ricardo Valencia",
            Document = "555111222",
            Party = "Coalicion Gamma",
            PhotoUrl = "https://photo.test/candidate3.png",
            ElectionId = election.Id
        };

        var candidate4 = new Candidate
        {
            Id = Guid.NewGuid(),
            Name = "Ana Torres",
            Document = "333888777",
            Party = "Movimiento Delta",
            PhotoUrl = "https://photo.test/candidate4.png",
            ElectionId = election.Id
        };

        var voters = new List<Voter>
        {
            new() { Id = Guid.NewGuid(), Name = "Juan Gomez", Document = "100001", ElectionId = election.Id },
            new() { Id = Guid.NewGuid(), Name = "Maria Lopez", Document = "100002", ElectionId = election.Id },
            new() { Id = Guid.NewGuid(), Name = "Andres Mora", Document = "100003", ElectionId = election.Id },
            new() { Id = Guid.NewGuid(), Name = "Sara Beltran", Document = "100004", ElectionId = election.Id }
        };

        await context.Elections.AddAsync(election);

        await context.VotingPlaces.AddAsync(votingPlace);

        await context.VotingTables.AddRangeAsync(tables);

        await context.VotingTerminals.AddRangeAsync(terminals);

        await context.Candidates.AddRangeAsync(candidate1, candidate2, candidate3, candidate4);

        await context.Voters.AddRangeAsync(voters);

        await context.SaveChangesAsync();
    }
}