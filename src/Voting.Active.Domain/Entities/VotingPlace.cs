using Voting.Active.Domain.Common;
using Voting.Active.Domain.Enums;

namespace Voting.Active.Domain.Entities;

public class VotingPlace : BaseEntity
{
    public string Name { get; set; } = default!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public OperationalStatus Status { get; set; } = OperationalStatus.Active;

    public ICollection<VotingTable> VotingTables { get; set; } = [];

    public bool IsActive { get; set; } = true;
}