namespace Voting.Active.Application.DTOs;

public class VoteDto
{
    public Guid VoteId { get; set; }

    public string CandidateName { get; set; } = default!;

    public Guid VoterId { get; set; }

    public Guid TerminalId { get; set; }

    public Guid TableId { get; set; }

    public Guid PlaceId { get; set; }

    public bool IsValid { get; set; }
}