using EcoBridge.Data;
using EcoBridge.DTO;
using EcoBridgeAPI.Services.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoBridge.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(IStatisticsService _statisticsService) : ControllerBase
{
    [HttpGet("stats")]
    [Route("/")]
    public async Task<ActionResult<AdminStatsDTO>> GetStats(CancellationToken cancellationToken)
    {
        var dto = await _statisticsService.GetAdminStatsAsync(cancellationToken);
        return Ok(dto);
    }
}

