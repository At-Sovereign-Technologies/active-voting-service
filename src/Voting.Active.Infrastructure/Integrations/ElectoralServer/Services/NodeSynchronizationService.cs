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

        await SyncElectionAsync(config.Election);

        int placesSynced = 0, tablesSynced = 0, terminalsSynced = 0;

        foreach (var placeDto in config.VotingPlaces)
        {
            await SyncVotingPlaceAsync(placeDto);
            placesSynced++;

            foreach (var tableDto in placeDto.Tables)
            {
                await SyncVotingTableAsync(tableDto);
                tablesSynced++;

                foreach (var terminalDto in tableDto.Terminals)
                {
                    await SyncVotingTerminalAsync(terminalDto);
                    terminalsSynced++;
                }
            }
        }

        int candidatesSynced = 0;
        foreach (var dto in config.Candidates)
        {
            await SyncCandidateAsync(dto);
            candidatesSynced++;
        }

        int votersSynced = 0;
        foreach (var dto in config.Voters)
        {
            await SyncVoterAsync(dto);
            votersSynced++;
        }

        int jurorsSynced = 0;
        foreach (var dto in config.Jurors)
        {
            await SyncJurorAsync(dto);
            jurorsSynced++;
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

    private async Task SyncElectionAsync(ElectionDto dto)
    {
        var existing = await _context.Elections.FindAsync(dto.Id);

        if (existing is not null)
        {
            existing.Name = dto.Name;
            existing.ElectionType = dto.ElectionType;
            existing.StartDate = dto.StartDate;
            existing.EndDate = dto.EndDate;
            existing.UpdatedAt = DateTime.UtcNow;
            return;
        }

        await _context.Elections.AddAsync(new Election
        {
            Id = dto.Id,
            Name = dto.Name,
            ElectionType = dto.ElectionType,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        });
    }

    private async Task SyncVotingPlaceAsync(VotingPlaceDto dto)
    {
        var existing = await _context.VotingPlaces.FindAsync(dto.Id);

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
            Id = dto.Id,
            Name = dto.Name,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsActive = dto.IsActive,
            Status = dto.IsActive ? OperationalStatus.Active : OperationalStatus.Suspended
        });
    }

    private async Task SyncVotingTableAsync(VotingTableDto dto)
    {
        var existing = await _context.VotingTables.FindAsync(dto.Id);

        if (existing is not null)
        {
            existing.Code = dto.Code;
            existing.IsActive = dto.IsActive;
            existing.VotingPlaceId = dto.VotingPlaceId;
            existing.UpdatedAt = DateTime.UtcNow;
            return;
        }

        await _context.VotingTables.AddAsync(new VotingTable
        {
            Id = dto.Id,
            Code = dto.Code,
            IsActive = dto.IsActive,
            VotingPlaceId = dto.VotingPlaceId,
            Status = dto.IsActive ? OperationalStatus.Active : OperationalStatus.Suspended
        });
    }

    private async Task SyncVotingTerminalAsync(VotingTerminalDto dto)
    {
        var existing = await _context.VotingTerminals.FindAsync(dto.Id);

        if (existing is not null)
        {
            existing.Secret = dto.Secret;
            existing.PublicKey = dto.PublicKey;
            existing.IsActive = dto.IsActive;
            existing.VotingTableId = dto.VotingTableId;
            existing.UpdatedAt = DateTime.UtcNow;
            return;
        }

        await _context.VotingTerminals.AddAsync(new VotingTerminal
        {
            Id = dto.Id,
            Secret = dto.Secret,
            PublicKey = dto.PublicKey,
            IsActive = dto.IsActive,
            VotingTableId = dto.VotingTableId,
            Status = dto.IsActive ? OperationalStatus.Active : OperationalStatus.Suspended
        });
    }

    private async Task SyncCandidateAsync(CandidateDto dto)
    {
        var existing = await _context.Candidates.FindAsync(dto.Id);

        if (existing is not null)
        {
            existing.Name = dto.Name;
            existing.Document = dto.Document;
            existing.Party = dto.Party;
            existing.PhotoUrl = dto.PhotoUrl;
            existing.ElectionId = dto.ElectionId;
            existing.UpdatedAt = DateTime.UtcNow;
            return;
        }

        await _context.Candidates.AddAsync(new Candidate
        {
            Id = dto.Id,
            Name = dto.Name,
            Document = dto.Document,
            Party = dto.Party,
            PhotoUrl = dto.PhotoUrl,
            ElectionId = dto.ElectionId
        });
    }

    private async Task SyncVoterAsync(VoterDto dto)
    {
        var existing = await _context.Voters.FindAsync(dto.Id);

        if (existing is not null)
        {
            existing.Name = dto.Name;
            existing.Document = dto.Document;
            existing.ElectionId = dto.ElectionId;
            existing.UpdatedAt = DateTime.UtcNow;
            // HasVoted is local state — never overwrite it from the server
            return;
        }

        // Handle case where a voter with the same document exists but different Id (e.g., from local seed data).
        // Remove the old record and re-insert with the canonical server Id, preserving HasVoted.
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
            Id = dto.Id,
            Name = dto.Name,
            Document = dto.Document,
            ElectionId = dto.ElectionId,
            HasVoted = preservedHasVoted
        });
    }

    private async Task SyncJurorAsync(JurorDto dto)
    {
        var existing = await _context.Jurors.FindAsync(dto.Id);

        if (existing is not null)
        {
            existing.Name = dto.Name;
            existing.Document = dto.Document;
            existing.ElectionId = dto.ElectionId;
            existing.UpdatedAt = DateTime.UtcNow;
            return;
        }

        await _context.Jurors.AddAsync(new Juror
        {
            Id = dto.Id,
            Name = dto.Name,
            Document = dto.Document,
            ElectionId = dto.ElectionId
        });
    }
}
