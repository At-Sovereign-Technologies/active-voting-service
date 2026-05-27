namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

public class CandidateDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string Document { get; set; } = default!;

    public string Party { get; set; } = default!;

    public string PhotoUrl { get; set; } = default!;

    public Guid ElectionId { get; set; }
}
