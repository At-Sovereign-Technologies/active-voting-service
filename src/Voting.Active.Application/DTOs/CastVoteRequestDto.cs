namespace Voting.Active.Application.DTOs;

public class CastVoteRequestDto
{
    public Guid VotingTerminalId { get; set; }

    public Guid VoterId { get; set; }

    public Guid CandidateId { get; set; }

    public string Signature { get; set; } = default!;
}