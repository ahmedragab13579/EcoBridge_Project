using EcoBridge.Data;
using EcoBridge.DTO;
using EcoBridgeAPI.Services.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoBridge.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(IStatisticsService _statisticsService) : ControllerBase
{
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var dto = await _statisticsService.GetAdminStatsAsync(cancellationToken);
        if(dto._success)
          return Ok(dto);
        return BadRequest(dto);
    }
}

