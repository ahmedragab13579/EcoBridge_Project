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

        [HttpPost("{id}/assign-volunteer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignVolunteer(int id, AssignVolunteerDTO dto)
        {
            var result = await _services.AssignVolunteer(id,dto);
            if(result._success)
                return Ok(result);
            return BadRequest(result);
           
        }


        [HttpPut("{id}/status")]
        [Authorize(Roles = "Volunteer")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var volunteerId))
                return Unauthorized("Invalid user context");

            var result = await _services.UpdateStatus(id, volunteerId, dto);
            if (result._success)
                return Ok(result);
            return BadRequest(result);

        }
    }
}
