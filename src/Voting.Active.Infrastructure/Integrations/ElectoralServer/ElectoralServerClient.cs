using System.Net.Http.Json;
using Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

namespace Voting.Active.Infrastructure.Integrations.ElectoralServer;

public class ElectoralServerClient
{
    private readonly HttpClient _httpClient;

    public ElectoralServerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<NodeConfigurationResponseDto> GetNodeConfigurationAsync()
    {
        var response = await _httpClient.GetAsync("/nodo");

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<NodeConfigurationResponseDto>();

        return result!;
    }
}
