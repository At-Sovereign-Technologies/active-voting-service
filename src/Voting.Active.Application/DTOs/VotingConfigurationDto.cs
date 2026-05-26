namespace Voting.Active.Application.DTOs;

public class VotingConfigurationDto
{
    public ElectionDto Election { get; set; } = default!;

    public List<CandidateDto> Candidates { get; set; } = [];

    public List<VoterDto> Voters { get; set; } = [];

    public VotingPlaceDto VotingPlace { get; set; } = default!;
}