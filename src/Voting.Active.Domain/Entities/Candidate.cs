using Voting.Active.Domain.Common;

namespace Voting.Active.Domain.Entities;

public class Candidate : BaseEntity
{
    public string Name { get; set; } = default!;

    public string Document { get; set; } = default!;

    public string Party { get; set; } = default!;

    public string PhotoUrl { get; set; } = default!;

    public Guid ElectionId { get; set; }

    public Election Election { get; set; } = default!;
}