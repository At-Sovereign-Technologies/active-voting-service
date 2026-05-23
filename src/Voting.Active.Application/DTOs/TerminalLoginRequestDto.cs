namespace Voting.Active.Application.DTOs;

public class TerminalLoginRequestDto
{
    public Guid TerminalId { get; set; }

    public string Secret { get; set; } = default!;
}