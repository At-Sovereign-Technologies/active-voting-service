namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

public class VotingTerminalDto
{
    public Guid Id { get; set; }

    public string Secret { get; set; } = default!;

    public string PublicKey { get; set; } = default!;

    public bool IsActive { get; set; }

    public Guid VotingTableId { get; set; }
}
