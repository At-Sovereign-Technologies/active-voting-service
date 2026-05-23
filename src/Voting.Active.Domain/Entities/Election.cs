using Voting.Active.Domain.Common;

namespace Voting.Active.Domain.Entities;

public class Election : BaseEntity
{
    public string Name { get; set; } = default!;

    public string ElectionType { get; set; } = default!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}