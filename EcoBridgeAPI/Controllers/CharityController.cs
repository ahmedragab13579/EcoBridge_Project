using EcoBridgeAPI.Services.Donation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoBridgeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Charity")]
    public class CharityController : ControllerBase
    {
        private readonly IDonationService _services;

        public CharityController(IDonationService services)
        {
            _services = services;
        }

        // =========================
        // Get All Pending Donations
        // =========================
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingDonations()
        {
            var result = await _services.GetPendingDonations(HttpContext.RequestAborted);

            if (result.Success)
                return Ok(result.Value);

            if (!string.IsNullOrWhiteSpace(result.Message) && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result.Message);

            return BadRequest(result.Message);
        }

        //=========================
        //Accept Donation
        //=========================
        [HttpPost("accept/{id}")]
        public async Task<IActionResult> AcceptDonation(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var charityId))
                return Unauthorized("Invalid user context");

            var result = await _services.AcceptDonation(id, charityId, HttpContext.RequestAborted);

            if (result.Success)
                return Ok(result.Value);

            if (!string.IsNullOrWhiteSpace(result.Message) && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result.Message);

            return BadRequest(result.Message);
        }

    }
}