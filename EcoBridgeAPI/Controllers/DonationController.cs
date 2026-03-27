using EcoBridge.Data;
using EcoBridge.Domains.Enums;
using EcoBridgeAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcoBridgeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationController : ControllerBase
    {
        private readonly EcoBridgeDbContext _context;

        public DonationController(EcoBridgeDbContext context)
        {
            _context = context;
        }

        [HttpPost("{id}/assign-volunteer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignVolunteer(int id, AssignVolunteerDTO dto)
        {
            var donation = await _context.Donations.FindAsync(id);

            if (donation == null)
                return NotFound("Donation not found");

            if (donation.Status != DonationStatus.Accepted)
                return BadRequest("Donation must be Accepted first");

            if (donation.VolunteerId != null)
                return BadRequest("Volunteer already assigned");

            var volunteer = await _context.Volunteers
                .FirstOrDefaultAsync(v => v.AccountId == dto.VolunteerId);

            if (volunteer == null)
                return NotFound("Volunteer not found");

            donation.VolunteerId = dto.VolunteerId;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Volunteer assigned successfully",
                donation.Id,
                donation.VolunteerId,
                Status = donation.Status.ToString()
            });
        }

        private bool IsValidStatusTransition(DonationStatus current, DonationStatus next)
        {
            return (current == DonationStatus.Accepted && next == DonationStatus.PickedUp)
                || (current == DonationStatus.PickedUp && next == DonationStatus.Delivered);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Volunteer")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDTO dto)
        {
            var donation = await _context.Donations.FindAsync(id);

            if (donation == null)
                return NotFound("Donation not found");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (donation.VolunteerId != userId)
                return Forbid("You can only update your assigned donations");

            if (!IsValidStatusTransition(donation.Status, dto.Status))
                return BadRequest("Invalid status transition");

            donation.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Status updated successfully",
                donation.Id,
                Status = donation.Status.ToString()
            });
        }
    }
}
