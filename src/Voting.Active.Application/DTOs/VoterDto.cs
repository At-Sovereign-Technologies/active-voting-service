namespace Voting.Active.Application.DTOs;

public class VoterDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string Document { get; set; } = default!;
}