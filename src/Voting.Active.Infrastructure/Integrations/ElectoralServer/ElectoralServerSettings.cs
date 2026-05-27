namespace Voting.Active.Infrastructure.Integrations.ElectoralServer;

public class ElectoralServerSettings
{
    public string BaseUrl { get; set; } = default!;

    public string NodeSecret { get; set; } = default!;
}
