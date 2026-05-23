using Voting.Active.Domain.Common;
using Voting.Active.Domain.Enums;

namespace Voting.Active.Domain.Entities;

public class VotingTerminal : BaseEntity
{
    public string Secret { get; set; } = default!;

    public string PublicKey { get; set; } = default!;

    public OperationalStatus Status { get; set; } = OperationalStatus.Active;

    public Guid VotingTableId { get; set; }

    public VotingTable VotingTable { get; set; } = default!;

    public bool IsActive { get; set; } = true;
}