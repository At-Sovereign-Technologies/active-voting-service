namespace Voting.Active.Application.DTOs;

public class VotingPlaceDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public List<VotingTerminalDto> Terminals { get; set; } = [];
}