using System.Text.Json.Serialization;

namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

public class CandidateDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nombre")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("documento")]
    public string Document { get; set; } = default!;

    [JsonPropertyName("partido")]
    public string Party { get; set; } = default!;

    [JsonPropertyName("foto_url")]
    public string PhotoUrl { get; set; } = default!;
}
