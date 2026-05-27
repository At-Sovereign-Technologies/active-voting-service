using System.Text.Json.Serialization;

namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

public class VotingPlaceDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nombre")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("latitud")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitud")]
    public double Longitude { get; set; }

    [JsonPropertyName("activo")]
    public bool IsActive { get; set; }

    [JsonPropertyName("terminales")]
    public List<VotingTerminalDto> Terminals { get; set; } = [];

    [JsonPropertyName("jurados")]
    public List<JurorDto> Jurors { get; set; } = [];
}
