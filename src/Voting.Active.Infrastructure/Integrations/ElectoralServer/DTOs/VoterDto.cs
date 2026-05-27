using System.Text.Json.Serialization;

namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

public class VoterDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nombre")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("documento")]
    public string Document { get; set; } = default!;
}
