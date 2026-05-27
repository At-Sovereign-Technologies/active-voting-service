namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

public class VotingPlaceDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public bool IsActive { get; set; }

    public List<VotingTableDto> Tables { get; set; } = [];
}
