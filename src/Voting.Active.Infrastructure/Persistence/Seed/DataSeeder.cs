using Microsoft.EntityFrameworkCore;
using Voting.Active.Domain.Entities;
using Voting.Active.Domain.Enums;

namespace Voting.Active.Infrastructure.Persistence.Seed;

public static class DataSeeder
{
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

        var table1 = new VotingTable
        {
            Id = Guid.NewGuid(),
            Code = "MESA-001",
            VotingPlaceId = votingPlace.Id,
            Status = OperationalStatus.Active
        };

        var terminal1 = new VotingTerminal
        {
            Id = Guid.NewGuid(),
            Secret = Guid.NewGuid().ToString(),
            PublicKey = "PUBLIC_KEY_TEST",
            VotingTableId = table1.Id,
            Status = OperationalStatus.Active
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

        var voter1 = new Voter
        {
            Id = Guid.NewGuid(),
            Name = "Juan Gomez",
            Document = "100001",
            ElectionId = election.Id
        };

        var voter2 = new Voter
        {
            Id = Guid.NewGuid(),
            Name = "Maria Lopez",
            Document = "100002",
            ElectionId = election.Id
        };

        await context.Elections.AddAsync(election);

        await context.VotingPlaces.AddAsync(votingPlace);

        await context.VotingTables.AddAsync(table1);

        await context.VotingTerminals.AddAsync(terminal1);

        await context.Candidates.AddRangeAsync(candidate1, candidate2);

        await context.Voters.AddRangeAsync(voter1, voter2);

        await context.SaveChangesAsync();
    }
}