namespace Voting.Active.Application.DTOs;

public class CastVoteResponseDto
{
    public bool Success { get; set; }

    public string Message { get; set; } = default!;
}