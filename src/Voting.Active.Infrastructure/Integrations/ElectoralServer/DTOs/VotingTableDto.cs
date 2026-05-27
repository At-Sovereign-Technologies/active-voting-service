namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

public class VotingTableDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = default!;

    public bool IsActive { get; set; }

    public Guid VotingPlaceId { get; set; }

    public List<VotingTerminalDto> Terminals { get; set; } = [];
}
