namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

public class NodeConfigurationResponseDto
{
    public ElectionDto Election { get; set; } = default!;

    public List<CandidateDto> Candidates { get; set; } = [];

    public List<VotingPlaceDto> VotingPlaces { get; set; } = [];

    public List<VoterDto> Voters { get; set; } = [];

    public List<JurorDto> Jurors { get; set; } = [];
}
