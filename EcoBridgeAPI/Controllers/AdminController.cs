using EcoBridgeAPI.DTO;
using EcoBridgeAPI.Services.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoBridgeAPI.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(IStatisticsService _statisticsService) : ControllerBase
{
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var dto = await _statisticsService.GetAdminStatsAsync(cancellationToken);
        if (dto.Success)
            return Ok(dto.Value);
        return BadRequest(dto.Message);
    }
}

