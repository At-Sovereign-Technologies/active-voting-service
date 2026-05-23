using Voting.Active.Domain.Common;

namespace Voting.Active.Domain.Entities;

public class Vote : BaseEntity
{
    public Guid VotingTerminalId { get; set; }

    public VotingTerminal VotingTerminal { get; set; } = default!;

    public Guid VotingTableId { get; set; }

    public VotingTable VotingTable { get; set; } = default!;

    public Guid VotingPlaceId { get; set; }

    public VotingPlace VotingPlace { get; set; } = default!;

    public Guid VoterId { get; set; }

    public Voter Voter { get; set; } = default!;

    public Guid CandidateId { get; set; }

    public Candidate Candidate { get; set; } = default!;

    public string Signature { get; set; } = default!;

    public bool IsValid { get; set; } = true;
}