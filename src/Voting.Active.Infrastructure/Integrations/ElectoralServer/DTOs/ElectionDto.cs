using System.Text.Json.Serialization;

namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

public class ElectionDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nombre")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("tipo_eleccion")]
    public string ElectionType { get; set; } = default!;

    [JsonPropertyName("fecha_inicio")]
    public long StartDateUnix { get; set; }

    [JsonPropertyName("fecha_fin")]
    public long EndDateUnix { get; set; }
}
