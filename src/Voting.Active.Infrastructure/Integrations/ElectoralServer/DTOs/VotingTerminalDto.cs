using System.Text.Json.Serialization;

namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

public class VotingTerminalDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("secreto")]
    public string Secret { get; set; } = default!;

    [JsonPropertyName("clave_publica")]
    public string PublicKey { get; set; } = default!;

    [JsonPropertyName("activo")]
    public bool IsActive { get; set; }

    [JsonPropertyName("votantes")]
    public List<VoterDto> Voters { get; set; } = [];
}
