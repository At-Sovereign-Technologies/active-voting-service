namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

public class NodeSyncSummaryDto
{
    public bool ElectionSynced { get; set; }

    public int CandidatesSynced { get; set; }

    public int VotingPlacesSynced { get; set; }

    public int VotingTablesSynced { get; set; }

    public int VotingTerminalsSynced { get; set; }

    public int VotersSynced { get; set; }

    public int JurorsSynced { get; set; }

    public DateTime SynchronizedAt { get; set; }
}
