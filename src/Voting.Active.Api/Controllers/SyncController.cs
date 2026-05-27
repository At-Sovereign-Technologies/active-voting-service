using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voting.Active.Infrastructure.Integrations.ElectoralServer.DTOs;
using Voting.Active.Infrastructure.Integrations.ElectoralServer.Services;

namespace Voting.Active.Api.Controllers;

[ApiController]
[Route("sync")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly INodeSynchronizationService _syncService;

    public SyncController(INodeSynchronizationService syncService)
    {
        _syncService = syncService;
    }

    [HttpPost("node")]
    public async Task<ActionResult<NodeSyncSummaryDto>> SyncNode()
    {
        var summary = await _syncService.SynchronizeAsync();

        return Ok(summary);
    }
}
