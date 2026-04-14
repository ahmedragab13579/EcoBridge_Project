using EcoBridgeAPI.DTO;
using EcoBridgeAPI.Services.Donation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoBridgeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationController : ControllerBase
    {
        private readonly IDonationService _services;

        public DonationController(IDonationService services)
        {
            _services = services;
        }

        [HttpPost]
        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> CreateDonation([FromForm] CreateDonationDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var accountId))
                return Unauthorized("Invalid user context");

            var result = await _services.CreateDonation(4, dto, HttpContext.RequestAborted);

            if (result.Success)
                return Ok(result.Value);

            if (!string.IsNullOrWhiteSpace(result.Message) && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result.Message);

            return BadRequest(result.Message);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> UpdateDonation(int id, [FromForm] UpdateDonationDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var accountId))
                return Unauthorized("Invalid user context");

            var result = await _services.UpdateDonation(id, accountId, dto, HttpContext.RequestAborted);

            if (result.Success)
                return NoContent();

            if (!string.IsNullOrWhiteSpace(result.Message) && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result.Message);

            return BadRequest(result.Message);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> DeleteDonation(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var accountId))
                return Unauthorized("Invalid user context");

            var result = await _services.DeleteDonation(id, accountId, HttpContext.RequestAborted);

            if (result.Success)
                return NoContent();

            if (!string.IsNullOrWhiteSpace(result.Message) && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result.Message);

            return BadRequest(result.Message);
        }

        [HttpPost("{id}/assign-volunteer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignVolunteer(int id, AssignVolunteerDTO dto)
        {
            var result = await _services.AssignVolunteer(id, dto, HttpContext.RequestAborted);
            if (result.Success)
                return Ok(result.Value);
            return BadRequest(result.Message);

        }


        [HttpPut("{id}/status")]
        [Authorize(Roles = "Volunteer")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var volunteerId))
                return Unauthorized("Invalid user context");

            var result = await _services.UpdateStatus(id, volunteerId, dto, HttpContext.RequestAborted);
            if (result.Success)
                return Ok(result.Value);
            return BadRequest(result.Message);

        }
    }
}
