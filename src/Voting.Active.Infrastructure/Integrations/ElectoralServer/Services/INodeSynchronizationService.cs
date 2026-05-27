using Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;

namespace Voting.Active.Infrastructure.Integrations.ElectoralServer.Services;

public interface INodeSynchronizationService
{
    Task<NodeSyncSummaryDto> SynchronizeAsync();
}
