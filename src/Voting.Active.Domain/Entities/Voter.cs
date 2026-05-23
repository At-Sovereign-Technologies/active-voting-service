using Voting.Active.Domain.Common;

namespace Voting.Active.Domain.Entities;

public class Voter : BaseEntity
{
    public string Name { get; set; } = default!;

    public string Document { get; set; } = default!;

    public bool HasVoted { get; set; } = false;

    public Guid ElectionId { get; set; }

    public Election Election { get; set; } = default!;
}