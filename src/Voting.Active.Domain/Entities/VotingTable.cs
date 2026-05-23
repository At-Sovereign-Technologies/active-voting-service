using Voting.Active.Domain.Common;
using Voting.Active.Domain.Enums;

namespace Voting.Active.Domain.Entities;

public class VotingTable : BaseEntity
{
    public string Code { get; set; } = default!;

    public Guid VotingPlaceId { get; set; }

    public VotingPlace VotingPlace { get; set; } = default!;

    public OperationalStatus Status { get; set; } = OperationalStatus.Active;

    public ICollection<VotingTerminal> VotingTerminals { get; set; } = [];

    public bool IsActive { get; set; } = true;}