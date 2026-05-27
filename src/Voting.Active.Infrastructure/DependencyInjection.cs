using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voting.Active.Infrastructure.Integrations.ElectoralServer;
using Voting.Active.Infrastructure.Integrations.ElectoralServer.Services;
using Voting.Active.Infrastructure.Persistence;

namespace Voting.Active.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres")
            );
        });

        var settings = configuration
            .GetSection("ElectoralServer")
            .Get<ElectoralServerSettings>()!;

        services.AddHttpClient<ElectoralServerClient>(client =>
        {
            client.BaseAddress = new Uri(settings.BaseUrl);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", settings.NodeSecret);
        });

        services.AddScoped<INodeSynchronizationService, NodeSynchronizationService>();

        return services;
    }
}
