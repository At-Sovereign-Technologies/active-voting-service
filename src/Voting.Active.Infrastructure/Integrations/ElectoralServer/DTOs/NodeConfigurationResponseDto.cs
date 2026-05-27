using System.Text.Json.Serialization;

namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

public class NodeConfigurationResponseDto
{
    [JsonPropertyName("eleccion")]
    public ElectionDto Election { get; set; } = default!;

    [JsonPropertyName("candidatos")]
    public List<CandidateDto> Candidates { get; set; } = [];

    [JsonPropertyName("puntos")]
    public List<VotingPlaceDto> VotingPlaces { get; set; } = [];
}
