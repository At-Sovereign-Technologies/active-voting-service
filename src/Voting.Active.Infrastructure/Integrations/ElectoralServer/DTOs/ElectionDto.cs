namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

public class ElectionDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string ElectionType { get; set; } = default!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}
