using Microsoft.EntityFrameworkCore;
using Voting.Active.Domain.Entities;
using Voting.Active.Domain.Enums;
using Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;
using Voting.Active.Infrastructure.Persistence;

namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.Services;

public class NodeSynchronizationService : INodeSynchronizationService
{
    private readonly ElectoralServerClient _client;
    private readonly ApplicationDbContext _context;

    public NodeSynchronizationService(
        ElectoralServerClient client,
        ApplicationDbContext context)
    {
        _client = client;
        _context = context;
    }

    public async Task<NodeSyncSummaryDto> SynchronizeAsync()
    {
        var config = await _client.GetNodeConfigurationAsync();
        var electionGuid = ToGuid(config.Election.Id);

        await SyncElectionAsync(config.Election, electionGuid);

        int placesSynced = 0, tablesSynced = 0, terminalsSynced = 0,
            votersSynced = 0, jurorsSynced = 0;

        foreach (var placeDto in config.VotingPlaces)
        {
            var placeGuid = ToGuid(placeDto.Id);
            await SyncVotingPlaceAsync(placeDto, placeGuid);
            placesSynced++;

            // The server has no tables — create one synthetic table per place
            var tableGuid = ToTableGuid(placeDto.Id);
            await SyncSyntheticTableAsync(tableGuid, placeGuid, placeDto.IsActive);
            tablesSynced++;

            foreach (var terminalDto in placeDto.Terminals)
            {
                await SyncVotingTerminalAsync(terminalDto, ToGuid(terminalDto.Id), tableGuid);
                terminalsSynced++;

                foreach (var voterDto in terminalDto.Voters)
                {
                    await SyncVoterAsync(voterDto, ToGuid(voterDto.Id), electionGuid);
                    votersSynced++;
                }
            }

            foreach (var jurorDto in placeDto.Jurors)
            {
                await SyncJurorAsync(jurorDto, ToGuid(jurorDto.Id), electionGuid);
                jurorsSynced++;
            }
        }

        int candidatesSynced = 0;
        foreach (var dto in config.Candidates)
        {
            await SyncCandidateAsync(dto, ToGuid(dto.Id), electionGuid);
            candidatesSynced++;
        }

        await _context.SaveChangesAsync();

        return new NodeSyncSummaryDto
        {
            ElectionSynced = true,
            CandidatesSynced = candidatesSynced,
            VotingPlacesSynced = placesSynced,
            VotingTablesSynced = tablesSynced,
            VotingTerminalsSynced = terminalsSynced,
            VotersSynced = votersSynced,
            JurorsSynced = jurorsSynced,
            SynchronizedAt = DateTime.UtcNow
        };
    }

    private async Task SyncElectionAsync(ElectionDto dto, Guid id)
    {
        var existing = await _context.Elections.FindAsync(id);

        if (existing is not null)
        {
            existing.Name = dto.Name;
            existing.ElectionType = dto.ElectionType;
            existing.StartDate = FromUnix(dto.StartDateUnix);
            existing.EndDate = FromUnix(dto.EndDateUnix);
            existing.UpdatedAt = DateTime.UtcNow;
            return;
        }

        await _context.Elections.AddAsync(new Election
        {
            Id = id,
            Name = dto.Name,
            ElectionType = dto.ElectionType,
            StartDate = FromUnix(dto.StartDateUnix),
            EndDate = FromUnix(dto.EndDateUnix)
        });
    }

    private async Task SyncVotingPlaceAsync(VotingPlaceDto dto, Guid id)
    {
        var existing = await _context.VotingPlaces.FindAsync(id);

        if (existing is not null)
        {
            existing.Name = dto.Name;
            existing.Latitude = dto.Latitude;
            existing.Longitude = dto.Longitude;
            existing.IsActive = dto.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            return;
        }

        await _context.VotingPlaces.AddAsync(new VotingPlace
        {
            Id = id,
            Name = dto.Name,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsActive = dto.IsActive,
            Status = dto.IsActive ? OperationalStatus.Active : OperationalStatus.Suspended
        });
    }

    private async Task SyncSyntheticTableAsync(Guid id, Guid placeId, bool isActive)
    {
        var existing = await _context.VotingTables.FindAsync(id);

        if (existing is not null)
        {
            existing.VotingPlaceId = placeId;
            existing.IsActive = isActive;
            existing.UpdatedAt = DateTime.UtcNow;
            return;
        }

        await _context.VotingTables.AddAsync(new VotingTable
        {
            Id = id,
            Code = $"MESA-{id.ToString()[..8]}",
            IsActive = isActive,
            VotingPlaceId = placeId,
            Status = isActive ? OperationalStatus.Active : OperationalStatus.Suspended
        });
    }

    private async Task SyncVotingTerminalAsync(VotingTerminalDto dto, Guid id, Guid tableId)
    {
        var existing = await _context.VotingTerminals.FindAsync(id);

        if (existing is not null)
        {
            existing.Secret = dto.Secret;
            existing.PublicKey = dto.PublicKey;
            existing.IsActive = dto.IsActive;
            existing.VotingTableId = tableId;
            existing.UpdatedAt = DateTime.UtcNow;
            return;
        }

        await _context.VotingTerminals.AddAsync(new VotingTerminal
        {
            Id = id,
            Secret = dto.Secret,
            PublicKey = dto.PublicKey,
            IsActive = dto.IsActive,
            VotingTableId = tableId,
            Status = dto.IsActive ? OperationalStatus.Active : OperationalStatus.Suspended
        });
    }

    private async Task SyncCandidateAsync(CandidateDto dto, Guid id, Guid electionId)
    {
        var existing = await _context.Candidates.FindAsync(id);

        if (existing is not null)
        {
            existing.Name = dto.Name;
            existing.Document = dto.Document;
            existing.Party = dto.Party;
            existing.PhotoUrl = dto.PhotoUrl;
            existing.ElectionId = electionId;
            existing.UpdatedAt = DateTime.UtcNow;
            return;
        }

        await _context.Candidates.AddAsync(new Candidate
        {
            Id = id,
            Name = dto.Name,
            Document = dto.Document,
            Party = dto.Party,
            PhotoUrl = dto.PhotoUrl,
            ElectionId = electionId
        });
    }

    private async Task SyncVoterAsync(VoterDto dto, Guid id, Guid electionId)
    {
        var existing = await _context.Voters.FindAsync(id);

        if (existing is not null)
        {
            existing.Name = dto.Name;
            existing.Document = dto.Document;
            existing.ElectionId = electionId;
            existing.UpdatedAt = DateTime.UtcNow;
            return;
        }

        var byDocument = await _context.Voters
            .FirstOrDefaultAsync(v => v.Document == dto.Document);

        bool preservedHasVoted = false;

        if (byDocument is not null)
        {
            preservedHasVoted = byDocument.HasVoted;
            _context.Voters.Remove(byDocument);
        }

        await _context.Voters.AddAsync(new Voter
        {
            Id = id,
            Name = dto.Name,
            Document = dto.Document,
            ElectionId = electionId,
            HasVoted = preservedHasVoted
        });
    }

    private async Task SyncJurorAsync(JurorDto dto, Guid id, Guid electionId)
    {
        var existing = await _context.Jurors.FindAsync(id);

        if (existing is not null)
        {
            existing.Name = dto.Name;
            existing.Document = dto.Document;
            existing.ElectionId = electionId;
            existing.UpdatedAt = DateTime.UtcNow;
            return;
        }

        await _context.Jurors.AddAsync(new Juror
        {
            Id = id,
            Name = dto.Name,
            Document = dto.Document,
            ElectionId = electionId
        });
    }

    // Converts a server int ID to a deterministic Guid
    static Guid ToGuid(int id) => new(id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    // Separate namespace for synthetic tables to avoid collision with place Guids
    static Guid ToTableGuid(int placeId) => new(placeId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);

    static DateTime FromUnix(long unix) =>
        DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime;
}
